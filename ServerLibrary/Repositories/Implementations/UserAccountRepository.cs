using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data.DbContexts;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerLibrary.Repositories.Implementations
{
    public class UserAccountRepository : IUserAccount
    {
        // For acces to the database
        private readonly AppDbContext _dbContext;
        // For acces into JwtSection for the secret key
        private readonly IOptions<JwtSection> _options;

        public UserAccountRepository(IOptions<JwtSection> options, AppDbContext context)
        {
            _options = options;
            _dbContext = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<GeneralResponse> CreateAccountAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return new GeneralResponse(false, "RegisterDto is null");
            }

            int comparePassword = string.Compare(registerDto.Password, registerDto.ConfirmPassword);

            if (comparePassword != 0)
            {
                return new GeneralResponse(false, "Passwords do not match");
            }

            // Check if user already exists
            var applicationUser = await FindUserByEmailAsync(registerDto.EmailAddress);

            if (applicationUser != null)
            {
                return new GeneralResponse(false, "User already exists");
            }

            // Create new user
            var user = new ApplicationUser
            {
                Email = registerDto.EmailAddress,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Name = registerDto.Fullname,
            };

            // Add user to the database
            bool success = await AddAndConfirmWithTransactionNewItemAsync(user);

            if (success)
            {
                // Check if user is admin
                bool isAdmin = user.Name.Equals(Constants.Admin, StringComparison.InvariantCultureIgnoreCase);

                // Create system and user roles for the user in the system or admin
                bool creatingWasSuccess = await CreateSystemAndUserRolesAsync(user.Id, isAdmin);

                if (creatingWasSuccess)
                {
                    return new GeneralResponse(true, "Account created successfully");
                }
            }

            return new GeneralResponse(false, "Account creation failed");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<LoginResponse> SingInAsync(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return new LoginResponse(false, "LoginDto is null");
            }

            // Verify email 
            var applicationUser = await FindUserByEmailAsync(loginDto.EmailAddress);    
            if(applicationUser == null)
            {
                return new LoginResponse(false, "User not found");
            }

            // Verify password
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(loginDto.Password, applicationUser.Password);
            if (!passwordMatch)
            {
                return new LoginResponse(false, "Invalid password");
            }

            // Get user role
            var userRole = await FindUserRoleAsync(applicationUser.Id);
            if (userRole == null)
            {
                return new LoginResponse(false, "User role not found");
            }

            // Get system role
            var systemRole = await FindSystemRoleAsync(userRole.RoleId);
            if (systemRole == null)
            {
                return new LoginResponse(false, "System role not found");
            }

            // Generate token
            string jwtToken = CreateToken(applicationUser, systemRole.Name);
            string refreshToken = CreateRefreshToken();

            var findUser = await _dbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UserId == applicationUser.Id);
            if(findUser == null)
            {
                // Add refresh token to database
                await AddAndConfirmWithTransactionNewItemAsync(new RefreshTokenInfo
                {
                    UserId = applicationUser.Id,
                    RefreshToken = refreshToken
                });
            }
            else
            {
                // Update refresh token
                findUser.RefreshToken = refreshToken;
                await _dbContext.SaveChangesAsync();
            }

            // Return response with token
            return new LoginResponse(true, "Login successful", jwtToken, refreshToken);
        }

        private string CreateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        /// <summary>
        /// Find user role by id
        /// </summary>
        /// <param name="applicationUserId"></param>
        /// <returns></returns>
        private async Task<UserRole?> FindUserRoleAsync(int applicationUserId)
        {
            return await _dbContext.Set<UserRole>().FirstOrDefaultAsync(a => a.UserId == applicationUserId);
        }

        /// <summary>
        /// Find system role by id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        private async Task<SystemRole?> FindSystemRoleAsync(int roleId)
        {
            return await _dbContext.Set<SystemRole>().FirstOrDefaultAsync(a => a.Id == roleId);
        }

        /// <summary>
        /// Create token for user, with claims and credentials
        /// It might be string.empty if the key is null
        /// </summary>
        /// <param name="appUser"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string CreateToken(ApplicationUser appUser, string name)
        {
            var key = _options.Value.Key;
            if (key != null) 
            {
                // Create security key and credentials
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // Create claims for the token
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, appUser.Name),
                    new Claim(ClaimTypes.Email, appUser.Email),
                    new Claim(ClaimTypes.Role, name)
                };

                // Create token
                var token = new JwtSecurityToken(
                    issuer: _options.Value.Issuer,
                    audience: _options.Value.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                    );

                // Return token
                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            return string.Empty;
        }

        /// <summary>
        /// Find user by email from ApplicationUsers
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<ApplicationUser?> FindUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return default;
            }

            // Find user by email 
            return await _dbContext
                        .ApplicationUsers
                        .FirstOrDefaultAsync(x => x.Email.ToLower()
                                                .Equals(email.ToLower()));
        }

        /// <summary>
        /// Add new item to database and confirm with transaction
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        private async Task<bool> AddAndConfirmWithTransactionNewItemAsync<TEntity>(TEntity item) where TEntity : class
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _dbContext.Set<TEntity>().AddAsync(item);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Log exception
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Create system and user roles, only one admin role can be created in the system
        /// and a lot of user roles can be created
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        private async Task<bool> CreateSystemAndUserRolesAsync(int userId, bool isAdmin)
        {
           if(isAdmin)
           {
                // Check if admin role exists
                var checkAdminRole = await _dbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name.Equals(Constants.Admin));
                if (checkAdminRole == null)
                {
                    // Add admin role
                    await AddAndConfirmWithTransactionNewItemAsync(new SystemRole
                    {
                        Name = Constants.Admin
                    });

                    // Get admin role id
                    int roleId = await _dbContext.SystemRoles
                        .Where(_ => _.Name.Equals(Constants.Admin))
                        .Select(_ => _.Id)
                        .FirstOrDefaultAsync();

                    // Add user role
                    await AddAndConfirmWithTransactionNewItemAsync(new UserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    });

                    return true;
                }

                return false;
           }
           else
           {
                // Add user role
                await AddAndConfirmWithTransactionNewItemAsync(new SystemRole
                {
                    Name = Constants.User
                });

                // Get user role id
                int roleId = await _dbContext.SystemRoles
                        .Where(_ => _.Name.Equals(Constants.User))
                        .OrderBy(_ => _.Id)
                        .Select(_ => _.Id)
                        .LastAsync();

                // Add user role
                await AddAndConfirmWithTransactionNewItemAsync(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });

                return true;
           }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            if(refreshTokenDto == null)
            {
                return new LoginResponse(false, "RefreshTokenDto is null");
            }

            // Find refresh token
            var findToken = await _dbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.RefreshToken.Equals(refreshTokenDto.RefreshToken));
            if(findToken == null)
            {
                return new LoginResponse(false, "Refresh token not found");
            }

            // Get user by id
            var currentUser = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(_ => _.Id == findToken.UserId);
            if(currentUser == null)
            {
                return new LoginResponse(false, "Refresh token could not be generated because user not found");
            }
            // Get user role
            var userRole = await FindUserRoleAsync(currentUser.Id);
            if(userRole == null)
            {
                return new LoginResponse(false, "User role not found");
            }

            // Get system role
            var systemRole = await FindSystemRoleAsync(userRole.RoleId);
            if (systemRole == null)
            {
                return new LoginResponse(false, "System role not found");
            }

            string jwtToken = CreateToken(currentUser, systemRole.Name);
            string refreshToken = CreateRefreshToken();

            var refreshTokenInfo = await _dbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.RefreshToken.Equals(refreshTokenDto.RefreshToken));
            if(refreshTokenInfo == null)
            {
                return new LoginResponse(false, "Refresh token could not be generated, because user has not been signed yet");
            }
            // Set new refresh token
            refreshTokenInfo.RefreshToken = refreshToken;

            // Save changes
            await _dbContext.SaveChangesAsync();

            return new LoginResponse(true, "Token refresh successfully", jwtToken, refreshToken);
        }
    }
}
