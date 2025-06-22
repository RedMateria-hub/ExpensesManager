using ExpensesManager.Database.Dtos;
using ExpensesManager.Database.Repos;

namespace ExpensesManager.Core.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllWithBsAsync();
            return entities.Select(a => new UserDto
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                Expenses = a.Expenses.Select(b => new ExpenseDto
                {
                    Id = b.Id,
                    Amount = b.Amount,
                    PaymentDate = b.PaymentDate,
                    Receiver = b.Receiver,
                    Category = new CategoryDto
                    {
                        Id = b.Category.Id,
                        Name = b.Category.Name,
                        Description = b.Category.Description
                    },
                    UserId = b.UserId,
                }).ToList()
            });
        }
    }
}
