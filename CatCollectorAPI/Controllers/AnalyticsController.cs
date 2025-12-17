using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CatCollectorAPI.Data;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly GameDbContext _db;
    public AnalyticsController(GameDbContext db) => _db = db;

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        if (!_db.Players.Any()) return Ok(new { message = "No players yet" });

        var totalPlayers = _db.Players.Count();
        var avgScore = _db.Players.Average(p => p.BestScore);
        var topPlayer = _db.Players.OrderByDescending(p => p.BestScore).FirstOrDefault()?.Name;
        var totalGood = _db.Players.Sum(p => p.GoodCatsCollected);
        var totalBad = _db.Players.Sum(p => p.BadCatsCollected);
        var totalChonky = _db.Players.Sum(p => p.ChonkyCatsCollected);

        return Ok(new { totalPlayers, avgScore, topPlayer, totalGood, totalBad, totalChonky });
    }
}
