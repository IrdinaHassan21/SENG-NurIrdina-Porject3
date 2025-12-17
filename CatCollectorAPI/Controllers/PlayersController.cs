using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CatCollectorAPI.Data;
using CatCollectorAPI.Models;

[ApiController]
[Route("api/players")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly GameDbContext _db;
    public PlayersController(GameDbContext db) => _db = db;

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // 🔹 Leaderboard
    [HttpGet]
    public IActionResult Get()
    {
        var players = _db.Players
            .OrderByDescending(p => p.BestScore)
            .Select(p => new
            {
                p.Name,
                p.BestScore,
                p.GoodCatsCollected,
                p.BadCatsCollected,
                p.ChonkyCatsCollected
            })
            .ToList();

        return Ok(players);
    }

    // 🔹 Create or get player for logged-in user
    [HttpPost("create")]
    public IActionResult CreatePlayer([FromBody] CreatePlayerDto dto)
    {
        int userId = GetUserId();

        var existing = _db.Players.FirstOrDefault(p => p.UserId == userId);
        if (existing != null)
            return Ok(existing);

        var player = new Player
        {
            Name = dto.Name,
            UserId = userId
        };

        _db.Players.Add(player);
        _db.SaveChangesAsync();
        return Ok(player);
    }

    // 🔹 Update scores safely
    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdatePlayerDto dto)
    {
        int userId = GetUserId();

        var p = _db.Players.FirstOrDefault(p => p.Id == id && p.UserId == userId);
        if (p == null)
            return Unauthorized();

        p.GoodCatsCollected += dto.GoodCatsCollected;
        p.BadCatsCollected += dto.BadCatsCollected;
        p.ChonkyCatsCollected += dto.ChonkyCatsCollected;

        if (dto.BestScore > p.BestScore)
            p.BestScore = dto.BestScore;

        _db.SaveChanges();
        return Ok(p);
    }
}

// DTO
public class CreatePlayerDto
{
    public string Name { get; set; } = "";
}
