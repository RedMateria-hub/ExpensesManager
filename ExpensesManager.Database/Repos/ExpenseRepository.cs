using ExpensesManager.Database.Context;
using ExpensesManager.Database.Dtos;
using ExpensesManager.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpensesManager.Database.Repos
{
    public interface IExpenseRepository
    {
        Task<PagedResult<Expense>> GetAllAsync(ExpenseQueryParameters parameters);
        Task<Expense?> GetByIdAsync(int id);
        Task<Expense> AddAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task DeleteAsync(Expense expense);

        Task<bool> UserExistsAsync(int userId);
        Task<bool> CategoryExistsAsync(int categoryId);
    }

    public class ExpenseRepository : IExpenseRepository
    {
        private readonly AppDbContext _context;

        public ExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Expense>> GetAllAsync(ExpenseQueryParameters parameters)
        {
            var query = _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(e => e.Receiver.Contains(parameters.SearchTerm) ||
                                        e.Category.Name.Contains(parameters.SearchTerm) ||
                                        e.User.Name.Contains(parameters.SearchTerm));
            }

            if (parameters.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == parameters.CategoryId.Value);
            }

            if (parameters.UserId.HasValue)
            {
                query = query.Where(e => e.UserId == parameters.UserId.Value);
            }

            //if (parameters.MinAmount.HasValue)
            //{
            //    query = query.Where(e => e.Amount >= (decimal)parameters.MinAmount.Value);
            //}

            //if (parameters.MaxAmount.HasValue)
            //{
            //    query = query.Where(e => e.Amount <= (decimal)parameters.MaxAmount.Value);
            //}

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(e => e.PaymentDate >= parameters.StartDate.Value);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(e => e.PaymentDate <= parameters.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Receiver))
            {
                query = query.Where(e => e.Receiver.Contains(parameters.Receiver));
            }

            query = ApplySorting(query, parameters.SortBy, parameters.SortOrder);

            var totalCount = await query.CountAsync();

            var expenses = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<Expense>
            {
                Data = expenses,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize),
                    HasPrevious = parameters.Page > 1,
                    HasNext = parameters.Page < (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
                }
            };
        }

        private IQueryable<Expense> ApplySorting(IQueryable<Expense> query, string sortBy, string sortOrder)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "PaymentDate";

            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy.ToLower() switch
            {
                "amount" => isDescending ? query.OrderByDescending(e => e.Amount) : query.OrderBy(e => e.Amount),
                "paymentdate" => isDescending ? query.OrderByDescending(e => e.PaymentDate) : query.OrderBy(e => e.PaymentDate),
                "receiver" => isDescending ? query.OrderByDescending(e => e.Receiver) : query.OrderBy(e => e.Receiver),
                "category" => isDescending ? query.OrderByDescending(e => e.Category.Name) : query.OrderBy(e => e.Category.Name),
                "user" => isDescending ? query.OrderByDescending(e => e.User.Name) : query.OrderBy(e => e.User.Name),
                _ => isDescending ? query.OrderByDescending(e => e.PaymentDate) : query.OrderBy(e => e.PaymentDate)
            };
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Expense> AddAsync(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task UpdateAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Expense expense)
        {
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.Id == categoryId);
        }
    }

}
