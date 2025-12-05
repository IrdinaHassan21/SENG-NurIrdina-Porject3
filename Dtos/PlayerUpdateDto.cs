using System.ComponentModel.DataAnnotations;

namespace CatCollector.API.Dtos
{
    public class PlayerUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int GoodCatsCollected { get; set; }
        public int BadCatsCollected { get; set; }
        public int ChonkyCatsCollected { get; set; }
        public int BestScore { get; set; }
    }
}
