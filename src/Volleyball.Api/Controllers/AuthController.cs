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
            // Return token and safe user info (exclude password hash)
            var userDto = new {
                id = user.Id,
                userName = user.UserName,
                role = user.Role,
                createdAt = user.CreatedAt
            };
            return Ok(new { token = tokenHandler.WriteToken(token), user = userDto });
        }
    }

    public class LoginRequest { public string UserName { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
}
