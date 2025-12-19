using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CatCollectorAPI.Data;
using CatCollectorAPI.Models;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config;

    public AuthController(GameDbContext db, ILogger<AuthController> logger, IConfiguration config) { _db = db; _logger = logger; _config = config; }

    private string GetJwtKey()
    {
        // Look up the key in configuration; fall back to a long default key for local dev
        var key = _config["Jwt:Key"] ?? "SUPER_SECRET_CAT_KEY_123_SUPER_SECRET_CAT_KEY_4567";
        // Ensure key is at least 32 bytes (256 bits) for HS256
        if (Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("JWT signing key is too short. Configure a key of at least 32 bytes in configuration (Jwt:Key).");
        return key;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        _logger.LogInformation("Register called with dto: {@Dto}", dto);

        if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Password))
        {
            _logger.LogWarning("Register failed: missing name or password");
            return BadRequest("Name and password are required");
        }

        // Case-insensitive name check using SQLite NOCASE collation (translatable)
        if (_db.Users.Any(u => EF.Functions.Collate(u.Name, "NOCASE") == dto.Name))
        {
            _logger.LogWarning("Register failed: user already exists name={Name}", dto.Name);
            return BadRequest("User already exists");
        }

        try
        {
            using var hmac = new HMACSHA256();

            var user = new User
            {
                Name = dto.Name,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password))
            };

            _db.Users.Add(user);
            var changed = await _db.SaveChangesAsync();
            _logger.LogInformation("Register succeeded: saved {Changed} entries for userId={UserId}", changed, user.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register failed with exception");
            return StatusCode(500, "Server error during registration");
        }
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Password))
        {
            _logger.LogWarning("Login failed: missing name or password");
            return BadRequest("Name and password are required");
        }

        _logger.LogInformation("Login attempt for name={Name}", dto.Name);

        // Case-insensitive lookup using SQLite NOCASE collation to ensure EF can translate
        var name = dto.Name;
        var user = _db.Users.FirstOrDefault(u => EF.Functions.Collate(u.Name, "NOCASE") == name);
        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found name={Name}", dto.Name);
            return Unauthorized("Invalid credentials");
        }

        using var hmac = new HMACSHA256(user.PasswordSalt);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));

        if (!hash.SequenceEqual(user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for name={Name}", dto?.Name);
            return Unauthorized("Invalid credentials");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name)
        };

        try
        {
            var key = GetJwtKey();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length < 32)
            {
                _logger.LogError("JWT key is too short ({Len} bytes) during login for userId={UserId}.", keyBytes.Length, user.Id);
                return StatusCode(500, "Server configuration error: JWT signing key is too short. Please set Jwt:Key to at least 32 bytes.");
            }

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256
                )
            );

            _logger.LogInformation("Login succeeded for userId={UserId} name={Name}", user.Id, user.Name);

            // Safely write the token and return
            try
            {
                var written = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { token = written });
            }
            catch (ArgumentOutOfRangeException aex)
            {
                _logger.LogError(aex, "JWT signing failed due to key size during WriteToken for userId={UserId}", user.Id);
                return StatusCode(500, "Server configuration error: JWT signing key invalid (too short). Please set Jwt:Key to at least 32 bytes.");
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "JWT key configuration error during token creation");
            return StatusCode(500, "Server configuration error: JWT signing key invalid. Please set Jwt:Key to at least 32 bytes in appsettings or environment variables.");
        }
    }
}
