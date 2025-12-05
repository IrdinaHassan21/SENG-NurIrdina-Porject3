using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatCollector.API.Models
{
    public class Player
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int GoodCatsCollected { get; set; }
        public int BadCatsCollected { get; set; }
        public int ChonkyCatsCollected { get; set; }
        public int BestScore { get; set; }

        [NotMapped]
        public int TotalCats => GoodCatsCollected + BadCatsCollected + ChonkyCatsCollected;
    }
}
