using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Managers;
using Server.Managers.Interfaces;
using ServerLibrary.Data.DbContexts;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Implementations;
using ServerLibrary.Repositories.Interfaces;
using System.Text;

const string corsBlazorWasm = "AllowBlazorWasm";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get connection string 
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if(connectionString == null)
{
    throw new NullReferenceException("Connection string not found");
}

// Add db context as AppDbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Register JwtSection
builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSection"));
var jwtSection = builder.Configuration.GetSection("JwtSection").Get<JwtSection>();
if(jwtSection == null)
{
    throw new NullReferenceException("JwtSection not found");
}

/// Register Services
builder.Services.AddScoped<IUserAccount, UserAccountRepository>();
builder.Services.AddScoped<IAuthenticationManager, AuthenticationManager>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsBlazorWasm,
        builder =>
        {
            builder.WithOrigins("https://localhost:7039", "http://localhost:5151")
                .AllowAnyHeader()
                .AllowCredentials()
                .AllowAnyMethod();
        });
});

// Add Authentication with JWT Bearer
builder.Services.AddAuthentication(options =>
{
    // Authentication scheme we want to use during authentication
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // Challenge for user when is not login in the syste and is required
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection.Issuer,
        ValidAudience = jwtSection.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.Key!))
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middlewares
app.UseHttpsRedirection();
app.UseCors(corsBlazorWasm);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
