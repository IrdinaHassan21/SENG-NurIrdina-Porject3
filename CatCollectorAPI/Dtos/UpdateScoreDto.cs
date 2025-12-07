using System.ComponentModel.DataAnnotations;

namespace CatCollector.API.Dtos
{
    public class UpdateScoreDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public int BestScore { get; set; }
        public int GoodCats { get; set; }
        public int BadCats { get; set; }
        public int ChonkyCats { get; set; }
    }
}
