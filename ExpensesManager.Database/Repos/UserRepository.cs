using ExpensesManager.Database.Context;
using ExpensesManager.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpensesManager.Database.Repos
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllWithBsAsync();
        Task<User> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllWithBsAsync()
        {
            return await _context.Users
                .Include(u => u.Expenses)
                .ThenInclude(e => e.Category)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Expenses)
                .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(user.Id) ?? user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            var deletedRows = await _context.SaveChangesAsync();
            return deletedRows > 0;
        }
    }
}
