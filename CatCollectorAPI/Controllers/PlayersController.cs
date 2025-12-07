using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CatCollectorAPI.Data;
using CatCollectorAPI.DTOs;
using CatCollectorAPI.Models;

namespace CatCollectorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly GameDbContext _db;

        public PlayersController(GameDbContext db) => _db = db;

        // GET: api/players
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var players = await _db.Players
                .OrderByDescending(p => p.BestScore)
                .ToListAsync();
            return Ok(players);
        }

        // GET: api/players/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var p = await _db.Players.FindAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        // POST: api/players
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(PlayerCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");
            if (await _db.Players.AnyAsync(p => p.Name.ToLower() == dto.Name.Trim().ToLower()))
                return BadRequest("Player name already exists");

            var player = new Player { Name = dto.Name.Trim() };
            _db.Players.Add(player);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = player.Id }, player);
        }

        // PUT: api/players/{id} (update full)
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, PlayerUpdateDto dto)
        {
            var p = await _db.Players.FindAsync(id);
            if (p == null) return NotFound();

            if (dto.BestScore < 0 || dto.GoodCatsCollected < 0 || dto.BadCatsCollected < 0 || dto.FatCatsCollected < 0)
                return BadRequest("Counts cannot be negative");

            p.BestScore = dto.BestScore;
            p.GoodCatsCollected = dto.GoodCatsCollected;
            p.BadCatsCollected = dto.BadCatsCollected;
            p.FatCatsCollected = dto.FatCatsCollected;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/players/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var p = await _db.Players.FindAsync(id);
            if (p == null) return NotFound();
            _db.Players.Remove(p);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/players/update-score
        // Upsert by name — intended for the frontend to call when game ends
        [Authorize]
        [HttpPut("update-score")]
        public async Task<IActionResult> UpsertByName([FromBody] PlayerUpsertDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required");
            if (dto.BestScore < 0 || dto.GoodCats < 0 || dto.BadCats < 0 || dto.FatCats < 0)
                return BadRequest("Counts cannot be negative");

            var name = dto.Name.Trim();
            var p = await _db.Players.SingleOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

            if (p == null)
            {
                p = new Player
                {
                    Name = name,
                    BestScore = dto.BestScore,
                    GoodCatsCollected = dto.GoodCats,
                    BadCatsCollected = dto.BadCats,
                    FatCatsCollected = dto.FatCats
                };
                _db.Players.Add(p);
            }
            else
            {
                // keep BestScore as the max
                p.GoodCatsCollected += dto.GoodCats;
                p.BadCatsCollected += dto.BadCats;
                p.FatCatsCollected += dto.FatCats;
                if (dto.BestScore > p.BestScore) p.BestScore = dto.BestScore;
            }

            await _db.SaveChangesAsync();
            return Ok(p);
        }
    }

    // DTO used by upsert endpoint
    public class PlayerUpsertDto
    {
        public string Name { get; set; } = string.Empty;
        public int BestScore { get; set; }
        public int GoodCats { get; set; }
        public int BadCats { get; set; }
        public int FatCats { get; set; }
    }
}
