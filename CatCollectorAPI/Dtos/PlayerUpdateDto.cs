using System.ComponentModel.DataAnnotations;

namespace CatCollectorAPI.DTOs
{
    public class PlayerUpdateDto
    {
        [Range(0, int.MaxValue)]
        public int BestScore { get; set; }

        [Range(0, int.MaxValue)]
        public int GoodCatsCollected { get; set; }

        [Range(0, int.MaxValue)]
        public int BadCatsCollected { get; set; }

        [Range(0, int.MaxValue)]
        public int FatCatsCollected { get; set; }
    }
}
