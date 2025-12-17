using CatCollectorConsole.Models;
using CatCollectorConsole.Services;

var service = new PlayerService();
service.GenerateRandomPlayers(5);

while (true)
{
    Console.WriteLine("\n=== CAT COLLECTOR CONSOLE ===");
    Console.WriteLine("1. View Players");
    Console.WriteLine("2. Add Player");
    Console.WriteLine("3. Update Player");
    Console.WriteLine("4. Delete Player");
    Console.WriteLine("5. Analytics / Statistics");
    Console.WriteLine("0. Exit");

    Console.Write("Select: ");
    var choice = Console.ReadLine();

    if (choice == "0") break;

    switch (choice)
    {
        case "1":
            foreach (var p in service.GetAll())
                Console.WriteLine($"{p.Id} | {p.Name} | Score: {p.BestScore}");
            break;

        case "2":
            Console.Write("Name: ");
            var name = Console.ReadLine();
            service.Add(new Player { Name = name! });
            break;

        case "3":
            Console.Write("Player ID: ");
            int id = int.Parse(Console.ReadLine()!);
            Console.Write("Best Score: ");
            int score = int.Parse(Console.ReadLine()!);
            service.Update(id, new Player { BestScore = score });
            break;

        case "4":
            Console.Write("Player ID: ");
            service.Delete(int.Parse(Console.ReadLine()!));
            break;

        case "5":
            service.ShowStatistics();
            break;
    }
}
