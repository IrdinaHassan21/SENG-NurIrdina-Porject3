using CatCollectorConsole.Models;

namespace CatCollectorConsole.Services;

public class PlayerService
{
    private readonly List<Player> _players = new();
    private int _nextId = 1;
    private readonly Random _rand = new();

    // Generate random dataset
    public void GenerateRandomPlayers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _players.Add(new Player
            {
                Id = _nextId++,
                Name = $"Player{_nextId}",
                GoodCatsCollected = _rand.Next(0, 50),
                BadCatsCollected = _rand.Next(0, 20),
                ChonkyCatsCollected = _rand.Next(0, 15),
                BestScore = _rand.Next(0, 100)
            });
        }
    }

    public List<Player> GetAll() => _players;

    public void Add(Player p)
    {
        p.Id = _nextId++;
        _players.Add(p);
    }

    public void Update(int id, Player updated)
    {
        var p = _players.FirstOrDefault(x => x.Id == id);
        if (p == null) return;

        p.Name = updated.Name;
        p.GoodCatsCollected = updated.GoodCatsCollected;
        p.BadCatsCollected = updated.BadCatsCollected;
        p.ChonkyCatsCollected = updated.ChonkyCatsCollected;
        p.BestScore = updated.BestScore;
    }

    public void Delete(int id) => _players.RemoveAll(p => p.Id == id);

    public void ShowStatistics()
    {
        if (_players.Count == 0)
        {
            Console.WriteLine("No players available.");
            return;
        }

        Console.WriteLine($"Total Players: {_players.Count}");
        Console.WriteLine($"Average Best Score: {_players.Average(p => p.BestScore):0.00}");
        Console.WriteLine($"Top Player: {_players.OrderByDescending(p => p.BestScore).First().Name}");
        Console.WriteLine($"Total Good Cats: {_players.Sum(p => p.GoodCatsCollected)}");
        Console.WriteLine($"Total Bad Cats: {_players.Sum(p => p.BadCatsCollected)}");
        Console.WriteLine($"Total Chonky Cats: {_players.Sum(p => p.ChonkyCatsCollected)}");
    }
}
