using ExpensesManager.Database.Context;
using ExpensesManager.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpensesManager.Database.Repos
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllWithBsAsync();
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
    }
}
