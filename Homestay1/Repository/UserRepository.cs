using Homestay1.Data;
using Homestay1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homestay1.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<User>> GetAllAsync(string search = null)
        {
            var query = _db.Users.Include(u => u.Role).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Phone.Contains(search));
            return await query.ToListAsync();
        }

        public async Task<User> GetByIdAsync(int id)
            => await _db.Users.Include(u => u.Role)
                              .FirstOrDefaultAsync(u => u.UserID == id);

        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u != null)
            {
                _db.Users.Remove(u);
                await _db.SaveChangesAsync();
            }
        }
    }
}