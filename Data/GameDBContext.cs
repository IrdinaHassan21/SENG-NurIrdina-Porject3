using CatCollectorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCollectorAPI.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
