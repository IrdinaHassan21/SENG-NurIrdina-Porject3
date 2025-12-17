using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using CatCollectorAPI.Data;
using CatCollectorAPI.Models;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GameDbContext _db;
    private const string KEY = "SUPER_SECRET_CAT_KEY_123";

    public AuthController(GameDbContext db) => _db = db;

    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {
        if (_db.Users.Any(u => u.Name == dto.Name))
            return BadRequest("User already exists");

        using var hmac = new HMACSHA256();

        var user = new User
        {
            Name = dto.Name,
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password))
        };

        _db.Users.Add(user);
        _db.SaveChanges();
        return Ok();
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        var user = _db.Users.FirstOrDefault(u => u.Name == dto.Name);
        if (user == null) return Unauthorized();

        using var hmac = new HMACSHA256(user.PasswordSalt);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));

        if (!hash.SequenceEqual(user.PasswordHash))
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(6),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY)),
                SecurityAlgorithms.HmacSha256
            )
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}
