using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpensesManager.Database.Context;
using ExpensesManager.Database.Dtos;
using ExpensesManager.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpensesManager.Database.Repos
{
    public interface ICategoryRepository
    {
        Task<PagedResult<Category>> GetAllAsync(CategoryQueryParameters parameters);
        Task<Category?> GetByIdAsync(int id);
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
        Task<Category?> GetByIdWithExpensesAsync(int id);
    }


    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Category>> GetAllAsync(CategoryQueryParameters parameters)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(parameters.SearchTerm) ||
                                        c.Description.Contains(parameters.SearchTerm));
            }

            query = ApplySorting(query, parameters.SortBy, parameters.SortOrder);

            var totalCount = await query.CountAsync();

            var categories = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<Category>
            {
                Data = categories,
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

        private IQueryable<Category> ApplySorting(IQueryable<Category> query, string sortBy, string sortOrder)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "Name";

            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "description" => isDescending ? query.OrderByDescending(c => c.Description) : query.OrderBy(c => c.Description),
                _ => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
            };
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Expenses)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Category?> GetByIdWithExpensesAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

}
