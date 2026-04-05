using Microsoft.EntityFrameworkCore;
using Volleyball.Domain.Entities;

namespace Volleyball.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<PlayerDocument> PlayerDocuments { get; set; } = null!;
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique registration number
            modelBuilder.Entity<Player>().HasIndex(p => p.RegistrationNumber).IsUnique();

            // PlayerDocument relation
            modelBuilder.Entity<PlayerDocument>(b => {
                b.HasKey(d => d.Id);
                b.Property(d => d.Id).ValueGeneratedOnAdd();
                b.HasOne(d => d.Player).WithMany().HasForeignKey(d => d.PlayerId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
