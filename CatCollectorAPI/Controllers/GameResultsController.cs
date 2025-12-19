using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CatCollectorAPI.Data;
using CatCollectorAPI.DTOs;
using CatCollectorAPI.Models;

[ApiController]
[Route("api/gameresults")]
public class GameResultsController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly ILogger<GameResultsController> _logger;

    public GameResultsController(GameDbContext db, ILogger<GameResultsController> logger) { _db = db; _logger = logger; }

    // Public leaderboard of top sessions
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTop([FromQuery] int limit = 50)
    {
        var top = await _db.GameResults
            .OrderByDescending(g => g.Score)
            .ThenByDescending(g => g.PlayedAt)
            .Take(limit)
            .Select(g => new { g.Id, g.PlayerName, g.Score, g.GoodCatsCollected, g.BadCatsCollected, g.ChonkyCatsCollected, g.PlayedAt })
            .ToListAsync();

        return Ok(top);
    }

    // Save a session result (authenticated)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(GameResultDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var player = await _db.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            var playerName = player?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "Anonymous";

            var gr = new GameResult
            {
                PlayerId = player?.Id,
                PlayerName = playerName,
                Score = dto.Score,
                GoodCatsCollected = dto.GoodCatsCollected,
                BadCatsCollected = dto.BadCatsCollected,
                ChonkyCatsCollected = dto.ChonkyCatsCollected,
                PlayedAt = DateTime.UtcNow
            };

            _db.GameResults.Add(gr);
            var changed = await _db.SaveChangesAsync();
            _logger.LogInformation("Saved GameResult for user={UserId} changed={Changed}", userId, changed);
            return Ok(gr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save GameResult");
            return StatusCode(500, "Failed to save game result");
        }
    }
}
