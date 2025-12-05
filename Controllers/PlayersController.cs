using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatCollector.API.Data;
using CatCollector.API.Dtos;
using CatCollector.API.Models;

namespace CatCollector.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PlayersController(AppDbContext db) => _db = db;

        // GET: api/players
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var players = await _db.Players.OrderByDescending(p => p.BestScore).ToListAsync();
            return Ok(players);
        }

        // GET: api/players/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
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
            if (await _db.Players.AnyAsync(x => x.Name == dto.Name))
                return BadRequest(new { message = "Player name exists" });

            var player = new Player
            {
                Name = dto.Name,
                GoodCatsCollected = dto.GoodCatsCollected,
                BadCatsCollected = dto.BadCatsCollected,
                ChonkyCatsCollected = dto.ChonkyCatsCollected,
                BestScore = dto.BestScore
            };

            _db.Players.Add(player);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = player.Id }, player);
        }

        // PUT: api/players/{id}
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, PlayerUpdateDto dto)
        {
            var p = await _db.Players.FindAsync(id);
            if (p == null) return NotFound();

            p.Name = dto.Name;
            p.GoodCatsCollected = dto.GoodCatsCollected;
            p.BadCatsCollected = dto.BadCatsCollected;
            p.ChonkyCatsCollected = dto.ChonkyCatsCollected;
            p.BestScore = dto.BestScore;
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
        [Authorize]
        [HttpPut("update-score")]
        public async Task<IActionResult> UpsertByName(UpdateScoreDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest();

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
                    ChonkyCatsCollected = dto.ChonkyCats
                };
                _db.Players.Add(p);
            }
            else
            {
                p.GoodCatsCollected += dto.GoodCats;
                p.BadCatsCollected += dto.BadCats;
                p.ChonkyCatsCollected += dto.ChonkyCats;
                if (dto.BestScore > p.BestScore) p.BestScore = dto.BestScore;
            }

            await _db.SaveChangesAsync();
            return Ok(p);
        }
    }
}
