using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data.DbContexts;
using ServerLibrary.Helpers;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
