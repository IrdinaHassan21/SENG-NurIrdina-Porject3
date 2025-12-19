using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatCollectorAPI.Models;

public class GameResult
{
    public int Id { get; set; }

    // Optional link to the Player (aggregated record)
    public int? PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public Player? Player { get; set; }

    // Keep a copy of the player's name in case the Player entry changes
    [Required]
    public string PlayerName { get; set; } = "";

    public int Score { get; set; }
    public int GoodCatsCollected { get; set; }
    public int BadCatsCollected { get; set; }
    public int ChonkyCatsCollected { get; set; }

    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}
