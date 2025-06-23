using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpensesManager.Database.Dtos;
using ExpensesManager.Database.Entities;
using ExpensesManager.Database.Repos;

namespace ExpensesManager.Core.Services
{
    public interface IExpenseService
    {
        Task<List<Expense>> GetAllAsync();
        Task<Expense?> GetByIdAsync(int id);
        Task<Expense?> AddAsync(CreateExpenseDto dto);
        Task<bool> UpdateAsync(int id, CreateExpenseDto dto);
        Task<bool> DeleteAsync(int id);
    }
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repository;

        public ExpenseService(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Expense>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Expense?> AddAsync(CreateExpenseDto dto)
        {
            if (!await _repository.UserExistsAsync(dto.UserId) ||
                !await _repository.CategoryExistsAsync(dto.CategoryId))
                return null;

            var expense = new Expense
            {
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                Receiver = dto.Receiver,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId
            };

            return await _repository.AddAsync(expense);
        }


        public async Task<bool> UpdateAsync(int id, CreateExpenseDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            if (!await _repository.UserExistsAsync(dto.UserId) ||
                !await _repository.CategoryExistsAsync(dto.CategoryId))
                return false;

            existing.Amount = dto.Amount;
            existing.PaymentDate = dto.PaymentDate;
            existing.Receiver = dto.Receiver;
            existing.UserId = dto.UserId;
            existing.CategoryId = dto.CategoryId;

            await _repository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(existing);
            return true;
        }
    }

}
