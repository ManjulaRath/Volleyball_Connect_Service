using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using System.IO;
// using System.Collections.Generic; (not required)
using Volleyball.Application.Services;
using Volleyball.Application.Interfaces;
using Volleyball.Infrastructure.Data;
using Volleyball.Infrastructure.Repositories;
using Volleyball.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add CORS policy to allow cross-origin requests (preflight will be handled)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(conn));

builder.Services.AddScoped<IPlayerRepository, Volleyball.Infrastructure.Repositories.PlayerRepository>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<AdminUserRepository>();

// Register document service (local file store for development)
builder.Services.AddScoped<IDocumentService, LocalFileDocumentService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceThisWithASecretKey12345";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; })
    .AddJwtBearer(options => {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Volleyball Connect API v1");
    options.RoutePrefix = string.Empty; // serve Swagger UI at app root
});
// Enable CORS middleware before authentication/authorization to handle preflight requests
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
