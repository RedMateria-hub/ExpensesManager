using ExpensesManager.Core.Services;
using ExpensesManager.Database.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpensesManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized();
                }

                if (id != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only access your own data");
                }

                var result = await _service.GetByIdAsync(id);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Post(CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _service.CreateAsync(createUserDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest("Invalid user ID");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized();
                }

                if (id != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only update your own data");
                }

                var existingUser = await _service.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                var result = await _service.UpdateAsync(id, updateUserDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                var existingUser = await _service.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                var result = await _service.DeleteAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to delete user" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user", error = ex.Message });
            }
        }
    }
}