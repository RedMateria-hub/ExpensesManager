using BCrypt.Net;
using ExpensesManager.Database.Dtos;
using ExpensesManager.Database.Entities;
using ExpensesManager.Database.Repos;

namespace ExpensesManager.Core.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<UserDto> GetCurrentUserAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var token = _jwtService.GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToUserDto(user),
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "User",
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);
            var token = _jwtService.GenerateJwtToken(createdUser);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToUserDto(createdUser),
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<UserDto> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Expenses = user.Expenses?.Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    PaymentDate = e.PaymentDate,
                    Receiver = e.Receiver,
                    UserId = e.UserId,
                    Category = e.Category != null ? new CategoryDto
                    {
                        Id = e.Category.Id,
                        Name = e.Category.Name,
                        Description = e.Category.Description
                    } : null
                }).ToList() ?? new List<ExpenseDto>()
            };
        }
    }
}