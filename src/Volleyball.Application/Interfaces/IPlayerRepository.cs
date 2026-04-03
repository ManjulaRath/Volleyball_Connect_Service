using System.Linq;
using Volleyball.Domain.Entities;

namespace Volleyball.Application.Interfaces
{
    public interface IPlayerRepository
    {
        IQueryable<Player> Query();
        System.Threading.Tasks.Task AddAsync(Player player);
        System.Threading.Tasks.Task<Player?> GetByIdAsync(int id);
    }
}
