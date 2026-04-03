using Microsoft.EntityFrameworkCore;
using Volleyball.Domain.Entities;

namespace Volleyball.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;
    }
}
