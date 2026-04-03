using System.Linq;
using System.Threading.Tasks;
using Volleyball.Application.Interfaces;
using Volleyball.Domain.Entities;

namespace Volleyball.Application.Services
{
    public class PlayerService
    {
        private readonly IPlayerRepository _repo;
        public PlayerService(IPlayerRepository repo) { _repo = repo; }

        public async Task RegisterAsync(Player p) => await _repo.AddAsync(p);

        public IQueryable<Player> Search(string? q, string? position, string? gender, int? minAge, int? maxAge)
        {
            var query = _repo.Query();
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => p.FirstName.Contains(q) || p.LastName.Contains(q) || p.Email.Contains(q));
            }
            if (!string.IsNullOrEmpty(position)) query = query.Where(p => p.Position == position);
            if (!string.IsNullOrEmpty(gender)) query = query.Where(p => p.Gender == gender);
            if (minAge.HasValue)
            {
                var maxDob = System.DateTime.UtcNow.AddYears(-minAge.Value);
                query = query.Where(p => p.DateOfBirth <= maxDob);
            }
            if (maxAge.HasValue)
            {
                var minDob = System.DateTime.UtcNow.AddYears(-maxAge.Value - 1).AddDays(1);
                query = query.Where(p => p.DateOfBirth >= minDob);
            }
            return query;
        }
    }
}
