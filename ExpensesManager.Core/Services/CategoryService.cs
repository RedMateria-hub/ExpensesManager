﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpensesManager.Database.Dtos;
using ExpensesManager.Database.Entities;
using ExpensesManager.Database.Repos;

namespace ExpensesManager.Core.Services
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryDto>> GetAllAsync(CategoryQueryParameters parameters);
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> AddAsync(CreateCategoryDto dto);
        Task<bool> UpdateAsync(int id, CreateCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }


    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<CategoryDto>> GetAllAsync(CategoryQueryParameters parameters)
        {
            var result = await _repository.GetAllAsync(parameters);

            var categoryDtos = result.Data.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();

            return new PagedResult<CategoryDto>
            {
                Data = categoryDtos,
                Pagination = result.Pagination
            };
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                //Expenses = category.Expenses.Select(e => new ExpenseDto
                //{
                //    Id = e.Id,
                //    Amount = e.Amount,
                //    PaymentDate = e.PaymentDate,
                //    Receiver = e.Receiver,
                //    UserId = e.UserId
                //}).ToList()
            };
        }

        public async Task<CategoryDto> AddAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Expenses = new List<Expense>()
            };

            var created = await _repository.AddAsync(category);

            return new CategoryDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description
                //Expenses = new List<ExpenseDto>()
            };
        }

        public async Task<bool> UpdateAsync(int id, CreateCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return false;

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Expenses = new List<Expense>(); // se asigura ca ramane gol

            await _repository.UpdateAsync(category);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _repository.GetByIdWithExpensesAsync(id);
            if (category == null)
                return false;

            if (category.Expenses != null && category.Expenses.Any())
                throw new InvalidOperationException("Can't delete category because it has an expense associated with it.");

            await _repository.DeleteAsync(category);
            return true;
        }
    }



}
