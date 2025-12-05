using Microsoft.EntityFrameworkCore;
using CatCollector.API.Models;

namespace CatCollector.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}
