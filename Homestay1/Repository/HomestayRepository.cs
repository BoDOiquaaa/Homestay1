using Homestay1.Data;
using Homestay1.Models;
using Homestay1.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homestay1.Repositories
{
    public class HomestayRepository : IHomestayRepository
    {
        private readonly ApplicationDbContext _context;
        public HomestayRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Homestay>> GetAllAsync(string search = null)
        {
            var query = _context.Homestays.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h => h.Name.Contains(search) || h.Address.Contains(search));
            }
            return await query.OrderByDescending(h => h.CreatedAt).ToListAsync();
        }

        public async Task<Homestay> GetByIdAsync(int id)
        {
            return await _context.Homestays.FindAsync(id);
        }
        public async Task<Homestay> GetByIdWithRoomsAsync(int id)
        {
            return await _context.Homestays
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HomestayID == id);
        }
        public async Task AddAsync(Homestay homestay)
        {
            homestay.CreatedAt = DateTime.Now;
            await _context.Homestays.AddAsync(homestay);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Homestay homestay)
        {
            _context.Homestays.Update(homestay);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Homestays.FindAsync(id);
            if (entity != null)
            {
                _context.Homestays.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}