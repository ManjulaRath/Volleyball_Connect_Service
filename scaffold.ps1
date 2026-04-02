# PowerShell scaffold.ps1
dotnet new sln -n Volleyball

dotnet new classlib -n Volleyball.Domain -o src/Volleyball.Domain
dotnet new classlib -n Volleyball.Application -o src/Volleyball.Application
dotnet new classlib -n Volleyball.Infrastructure -o src/Volleyball.Infrastructure
dotnet new webapi -n Volleyball.Api -o src/Volleyball.Api --no-https

dotnet sln add src/Volleyball.Domain/Volleyball.Domain.csproj
dotnet sln add src/Volleyball.Application/Volleyball.Application.csproj
dotnet sln add src/Volleyball.Infrastructure/Volleyball.Infrastructure.csproj
dotnet sln add src/Volleyball.Api/Volleyball.Api.csproj

dotnet add src/Volleyball.Application/Volleyball.Application.csproj reference src/Volleyball.Domain/Volleyball.Domain.csproj
dotnet add src/Volleyball.Infrastructure/Volleyball.Infrastructure.csproj reference src/Volleyball.Application/Volleyball.Application.csproj
dotnet add src/Volleyball.Api/Volleyball.Api.csproj reference src/Volleyball.Infrastructure/Volleyball.Infrastructure.csproj

dotnet add src/Volleyball.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/Volleyball.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/Volleyball.Infrastructure package BCrypt.Net-Next
dotnet add src/Volleyball.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/Volleyball.Api package Swashbuckle.AspNetCore

# create directories
New-Item -ItemType Directory -Path src/Volleyball.Domain/Entities -Force | Out-Null
New-Item -ItemType Directory -Path src/Volleyball.Application/Interfaces -Force | Out-Null
New-Item -ItemType Directory -Path src/Volleyball.Application/Services -Force | Out-Null
New-Item -ItemType Directory -Path src/Volleyball.Infrastructure/Data -Force | Out-Null
New-Item -ItemType Directory -Path src/Volleyball.Infrastructure/Repositories -Force | Out-Null
New-Item -ItemType Directory -Path src/Volleyball.Api/Controllers -Force | Out-Null

# Add files (replace content if files already exist)
Set-Content -Path src/Volleyball.Domain/Entities/Player.cs -Value @'
using System;

namespace Volleyball.Domain.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
'@

Set-Content -Path src/Volleyball.Domain/Entities/AdminUser.cs -Value @'
using System;

namespace Volleyball.Domain.Entities
{
    public class AdminUser
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
'@

Set-Content -Path src/Volleyball.Application/Interfaces/IPlayerRepository.cs -Value @'
using System.Linq;
using Volleyball.Domain.Entities;

namespace Volleyball.Application.Interfaces
{
    public interface IPlayerRepository
    {
        IQueryable<Player> Query();
        System.Threading.Tasks.Task AddAsync(Player player);
        System.Threading.Tasks.Task<Player?> GetByIdAsync(int id);
    }
}
'@

Set-Content -Path src/Volleyball.Application/Services/PlayerService.cs -Value @'
using System.Linq;
using System.Threading.Tasks;
using Volleyball.Application.Interfaces;
using Volleyball.Domain.Entities;

namespace Volleyball.Application.Services
{
    public class PlayerService
    {
        private readonly IPlayerRepository _repo;
        public PlayerService(IPlayerRepository repo) { _repo = repo; }

        public async Task RegisterAsync(Player p) => await _repo.AddAsync(p);

        public IQueryable<Player> Search(string? q, string? position, string? gender, int? minAge, int? maxAge)
        {
            var query = _repo.Query();
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => p.FirstName.Contains(q) || p.LastName.Contains(q) || p.Email.Contains(q));
            }
            if (!string.IsNullOrEmpty(position)) query = query.Where(p => p.Position == position);
            if (!string.IsNullOrEmpty(gender)) query = query.Where(p => p.Gender == gender);
            if (minAge.HasValue)
            {
                var maxDob = System.DateTime.UtcNow.AddYears(-minAge.Value);
                query = query.Where(p => p.DateOfBirth <= maxDob);
            }
            if (maxAge.HasValue)
            {
                var minDob = System.DateTime.UtcNow.AddYears(-maxAge.Value - 1).AddDays(1);
                query = query.Where(p => p.DateOfBirth >= minDob);
            }
            return query;
        }
    }
}
'@

Set-Content -Path src/Volleyball.Infrastructure/Data/ApplicationDbContext.cs -Value @'
using Microsoft.EntityFrameworkCore;
using Volleyball.Domain.Entities;

namespace Volleyball.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;
    }
}
'@

Set-Content -Path src/Volleyball.Infrastructure/Repositories/PlayerRepository.cs -Value @'
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volleyball.Application.Interfaces;
using Volleyball.Domain.Entities;
using Volleyball.Infrastructure.Data;

namespace Volleyball.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _db;
        public PlayerRepository(ApplicationDbContext db) { _db = db; }
        public IQueryable<Player> Query() => _db.Players.AsNoTracking();
        public async Task AddAsync(Player player) { _db.Players.Add(player); await _db.SaveChangesAsync(); }
        public Task<Player?> GetByIdAsync(int id) => _db.Players.FirstOrDefaultAsync(p => p.Id == id);
    }
}
'@

Set-Content -Path src/Volleyball.Infrastructure/Repositories/AdminUserRepository.cs -Value @'
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volleyball.Domain.Entities;
using Volleyball.Infrastructure.Data;

namespace Volleyball.Infrastructure.Repositories
{
    public class AdminUserRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminUserRepository(ApplicationDbContext db) { _db = db; }
        public Task<AdminUser?> GetByUserNameAsync(string userName) => _db.AdminUsers.FirstOrDefaultAsync(u => u.UserName == userName);
    }
}
'@

Set-Content -Path src/Volleyball.Api/Controllers/PlayersController.cs -Value @'
using Microsoft.AspNetCore.Mvc;
using Volleyball.Application.Services;
using Volleyball.Domain.Entities;
using System.Linq;

namespace Volleyball.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerService _service;
        public PlayersController(PlayerService service) { _service = service; }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> Register([FromBody] Player p)
        {
            await _service.RegisterAsync(p);
            return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
        }

        [HttpGet]
        public IActionResult Search([FromQuery] string? q, [FromQuery] string? position, [FromQuery] string? gender, [FromQuery] int? minAge, [FromQuery] int? maxAge, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _service.Search(q, position, gender, minAge, maxAge);
            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var p = _service.Search(null, null, null, null, null).FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(p);
        }
    }
}
'@

Set-Content -Path src/Volleyball.Api/Controllers/AuthController.cs -Value @'
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Volleyball.Infrastructure.Repositories;

namespace Volleyball.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AdminUserRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(AdminUserRepository repo, IConfiguration config) { _repo = repo; _config = config; }

        [HttpPost("login")]
        public async System.Threading.Tasks.Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _repo.GetByUserNameAsync(req.UserName);
            if (user == null) return Unauthorized();
            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return Unauthorized();

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "ReplaceThisWithASecretKey12345");
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.Role, user.Role) }),
                Expires = System.DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }

    public class LoginRequest { public string UserName { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
}
'@

# Add Program.cs (replace existing if needed)
Set-Content -Path src/Volleyball.Api/Program.cs -Value @'
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Volleyball.Application.Services;
using Volleyball.Application.Interfaces;
using Volleyball.Infrastructure.Data;
using Volleyball.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=VolleyballDb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(conn));

builder.Services.AddScoped<IPlayerRepository, Volleyball.Infrastructure.Repositories.PlayerRepository>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<AdminUserRepository>();

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

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
'@

# Build
dotnet build