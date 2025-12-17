using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatCollectorAPI.Models;

public class Player
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    // 🔐 Link player to authenticated user
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public int GoodCatsCollected { get; set; }
    public int BadCatsCollected { get; set; }
    public int ChonkyCatsCollected { get; set; }
    public int BestScore { get; set; }
}
