using System;
using System.ComponentModel.DataAnnotations;

namespace CatCollectorAPI.Models
{
    public class Player
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // counts per cat type
        public int GoodCatsCollected { get; set; }
        public int BadCatsCollected { get; set; }

        // backend name is FatCatsCollected (chonky)
        public int FatCatsCollected { get; set; }

        public int BestScore { get; set; }
    }
}
