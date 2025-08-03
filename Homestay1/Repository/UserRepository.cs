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
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== REPOSITORY AddAsync ===");
                System.Diagnostics.Debug.WriteLine($"Adding user: {user.FullName} - {user.Email}");
                System.Diagnostics.Debug.WriteLine($"RoleID: {user.RoleID}");

                // Đảm bảo CreatedAt được set
                if (user.CreatedAt == default(DateTime))
                {
                    user.CreatedAt = DateTime.Now;
                }

                // Add user to context
                await _db.Users.AddAsync(user);
                System.Diagnostics.Debug.WriteLine("User added to context");

                // Save changes
                var result = await _db.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"SaveChanges result: {result} rows affected");

                // Log the generated UserID
                System.Diagnostics.Debug.WriteLine($"Generated UserID: {user.UserID}");

                if (result > 0)
                {
                    System.Diagnostics.Debug.WriteLine("✅ User successfully saved to database");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ No rows were affected - user may not have been saved");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception in AddAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }

                throw; // Re-throw to let controller handle it
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                _db.Users.Update(user);
                var result = await _db.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"UpdateAsync: {result} rows affected");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var u = await _db.Users.FindAsync(id);
                if (u != null)
                {
                    _db.Users.Remove(u);
                    var result = await _db.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"DeleteAsync: {result} rows affected");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in DeleteAsync: {ex.Message}");
                throw;
            }
        }
    }
}