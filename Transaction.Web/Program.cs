using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Transaction.Core;
using Transaction.Service.Options;
using Transaction.Service.Services;
using Transaction.Service.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Get the database connection string from environment variables or configuration
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
                       ?? builder.Configuration.GetConnectionString("ConnectionString");

// Configure the database context with PostgresSQL
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register services for dependency injection
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFileService, FileService>();

// Configure application settings from configuration files
builder.Services.Configure<DbConnectionString>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<TimeZoneOption>(builder.Configuration.GetSection("TimeZoneApiOption"));
builder.Services.AddTransient<HttpClient>();

// Add controllers and API endpoint exploration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Transaction API",
        Version = "v1",
        Description = "API for working with transactions"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();