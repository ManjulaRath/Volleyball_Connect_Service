using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volleyball.Application.Interfaces;
using Volleyball.Domain.Entities;
using Volleyball.Infrastructure.Data;

namespace Volleyball.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _db;
        public PlayerRepository(ApplicationDbContext db) { _db = db; }
        public IQueryable<Player> Query() => _db.Players.AsNoTracking();
        public async Task AddAsync(Player player) { _db.Players.Add(player); await _db.SaveChangesAsync(); }
        public Task<Player?> GetByIdAsync(int id) => _db.Players.FirstOrDefaultAsync(p => p.Id == id);
    }
}
