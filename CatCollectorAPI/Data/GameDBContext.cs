using Microsoft.EntityFrameworkCore;
using CatCollectorAPI.Models;

namespace CatCollectorAPI.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<AppUser> Users { get; set; } = null!;
    }

    // simple app user for auth
    public class AppUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public string Role { get; set; } = "User";
    }
}
