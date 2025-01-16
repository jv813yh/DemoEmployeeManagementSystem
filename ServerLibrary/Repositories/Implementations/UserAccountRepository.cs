using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServerLibrary.Data.DbContexts;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Interfaces;

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



            return new LoginResponse(true, "Login successful", "token");
        }

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
    }
}
