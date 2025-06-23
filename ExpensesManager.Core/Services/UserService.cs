using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using ExpensesManager.Database.Dtos;
using ExpensesManager.Database.Entities;
using ExpensesManager.Database.Repos;

namespace ExpensesManager.Core.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteAsync(int id);
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
        public async Task<UserDto> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            return entity != null ? MapToUserDto(entity) : null;
        }
        public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
        {
            if (await _repository.EmailExistsAsync(createUserDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password), // Hash password
                Role = "User",
                IsActive = true
            };

            var createdEntity = await _repository.CreateAsync(user);
            return MapToUserDto(createdEntity);
        }

        public async Task<UserDto> UpdateAsync(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _repository.GetByIdAsync(id);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {id} not found");
            }

            existingUser.Name = updateUserDto.Name;
            existingUser.Email = updateUserDto.Email;

            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            var updatedEntity = await _repository.UpdateAsync(existingUser);
            return MapToUserDto(updatedEntity);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
