namespace CatCollectorConsole.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int GoodCatsCollected { get; set; }
    public int BadCatsCollected { get; set; }
    public int ChonkyCatsCollected { get; set; }
    public int BestScore { get; set; }
}