using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CatCollectorAPI.Data;
using CatCollectorAPI.Models;

[ApiController]
[Route("api/players")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly ILogger<PlayersController> _logger;
    public PlayersController(GameDbContext db, ILogger<PlayersController> logger) { _db = db; _logger = logger; }

    private int GetUserId()
    {
        var idStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idStr))
            throw new InvalidOperationException("User identity is not available; ensure the request is authenticated.");
        return int.Parse(idStr);
    }

    // 🔹 Leaderboard (public)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var players = await _db.Players
            .OrderByDescending(p => p.BestScore)
            .Select(p => new
            {
                p.Name,
                p.BestScore,
                p.GoodCatsCollected,
                p.BadCatsCollected,
                p.ChonkyCatsCollected
            })
            .ToListAsync();

        return Ok(players);
    }

    // 🔹 Create or get player for logged-in user
    [HttpPost("create")]
    public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
        {
            _logger.LogWarning("CreatePlayer failed: missing name in request");
            return BadRequest("Name is required");
        }

        // Diagnostic logs to help debug auth issues
        var authHeader = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"].ToString() : null;
        _logger.LogInformation("CreatePlayer called. AuthHeader present={HasAuth} Header={HeaderSnippet}", !string.IsNullOrEmpty(authHeader), authHeader != null ? authHeader.Substring(0, Math.Min(64, authHeader.Length)) : "(null)");
        _logger.LogInformation("User.Identity.IsAuthenticated={IsAuth}", HttpContext.User?.Identity?.IsAuthenticated);
        foreach (var c in HttpContext.User?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
            _logger.LogInformation("Claim: {Type}={Value}", c.Type, c.Value);

        int userId = GetUserId();

        _logger.LogInformation("CreatePlayer called for userId={UserId} name={Name}", userId, dto.Name);

        var existing = await _db.Players.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null)
            return Ok(existing);

        var player = new Player
        {
            Name = dto.Name,
            UserId = userId
        };

        _db.Players.Add(player);
        var changed = await _db.SaveChangesAsync(); // <- await this properly
        _logger.LogInformation("CreatePlayer saved {Changed} entries. PlayerId={PlayerId}", changed, player.Id);

        return Ok(player);
    }

    // 🔹 Get current player's record for logged-in user
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // Diagnostic logging for auth troubleshooting
        var authHeader = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"].ToString() : null;
        _logger.LogInformation("Me called. AuthHeader present={HasAuth} HeaderSnippet={HeaderSnippet}", !string.IsNullOrEmpty(authHeader), authHeader != null ? authHeader.Substring(0, Math.Min(64, authHeader.Length)) : "(null)");
        _logger.LogInformation("User.Identity.IsAuthenticated={IsAuth}", HttpContext.User?.Identity?.IsAuthenticated);
        foreach (var c in HttpContext.User?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
            _logger.LogInformation("Claim: {Type}={Value}", c.Type, c.Value);

        int userId = GetUserId();
        var player = await _db.Players.FirstOrDefaultAsync(p => p.UserId == userId);
        if (player == null) return NotFound();
        return Ok(player);
    }

    // 🔹 Update scores safely
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePlayerDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Update failed: missing body for playerId={PlayerId}", id);
            return BadRequest("Update data required");
        }

        int userId = GetUserId();

        _logger.LogInformation("Update called for playerId={PlayerId} userId={UserId} dto={Dto}", id, userId, dto);

        var p = await _db.Players.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (p == null)
        {
            _logger.LogWarning("Update failed: player not found or not owned by user. playerId={PlayerId} userId={UserId}", id, userId);
            return Unauthorized("Player not found or not owned by user");
        }

        p.GoodCatsCollected += dto.GoodCatsCollected;
        p.BadCatsCollected += dto.BadCatsCollected;
        p.ChonkyCatsCollected += dto.ChonkyCatsCollected;

        if (dto.BestScore > p.BestScore)
            p.BestScore = dto.BestScore;

        var changed = await _db.SaveChangesAsync(); // <- await this properly
        _logger.LogInformation("Update saved {Changed} entries for playerId={PlayerId}", changed, p.Id);
        return Ok(p);
    }

    [HttpGet("debug")]
    public async Task<IActionResult> Debug()
    {
        try
        {
            var playersCount = await _db.Players.CountAsync();
            var usersCount = await _db.Users.CountAsync();
            var topPlayers = await _db.Players
                .OrderByDescending(p => p.BestScore)
                .Take(5)
                .Select(p => new { p.Name, p.BestScore, p.GoodCatsCollected, p.BadCatsCollected, p.ChonkyCatsCollected })
                .ToListAsync();

            var conn = _db.Database.GetDbConnection();
            var dbPath = conn?.DataSource ?? "unknown";

            _logger.LogInformation("Debug called: players={Players} users={Users} db={Db}", playersCount, usersCount, dbPath);

            return Ok(new { playersCount, usersCount, topPlayers, dbPath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debug endpoint failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// DTOs
public class CreatePlayerDto
{
    public string Name { get; set; } = "";
}
