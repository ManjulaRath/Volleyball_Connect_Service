using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volleyball.Domain.Entities;
using Volleyball.Infrastructure.Data;

namespace Volleyball.Infrastructure.Repositories
{
    public class AdminUserRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminUserRepository(ApplicationDbContext db) { _db = db; }
        public Task<AdminUser?> GetByUserNameAsync(string userName) => _db.AdminUsers.FirstOrDefaultAsync(u => u.UserName == userName);
    }
}
