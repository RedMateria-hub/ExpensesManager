using ExpensesManager.Core.Services;
using ExpensesManager.Database.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExpensesManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Get([FromQuery] CategoryQueryParameters parameters)
        {
            if (parameters == null)
            {
                return BadRequest("Query parameters cannot be null");
            }

            if (!TryValidateModel(parameters))
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.GetAllAsync(parameters);
                Response.Headers.Add("X-Pagination",
                    System.Text.Json.JsonSerializer.Serialize(result.Pagination));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Get([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive integer")] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                {
                    return NotFound($"Category with ID {id} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the category");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Post([FromBody] CreateCategoryDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Category data cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await IsValidCategoryData(dto))
            {
                return BadRequest("Invalid category data provided");
            }

            try
            {
                var result = await _service.AddAsync(dto);
                if (result == null)
                {
                    return BadRequest("Failed to create category");
                }

                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while creating the category");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Put(
            [Range(1, int.MaxValue, ErrorMessage = "ID must be a positive integer")] int id,
            [FromBody] CreateCategoryDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Category data cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await IsValidCategoryData(dto, id))
            {
                return BadRequest("Invalid category data provided");
            }

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (!updated)
                {
                    return NotFound($"Category with ID {id} not found or could not be updated");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the category");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive integer")] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound($"Category with ID {id} not found or could not be deleted");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict($"Cannot delete category: {ex.Message}"); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the category");
            }
        }

        private async Task<bool> IsValidCategoryData(CreateCategoryDto dto, int? excludeId = null)
        {
            var reservedNames = new[] { "system", "admin", "default", "temp", "temporary" };
            if (reservedNames.Contains(dto.Name?.ToLowerInvariant()))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name) &&
                dto.Name.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c) && c != '-' && c != '_'))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 1000)
            {
                return false;
            }

            return true;
        }

        private bool IsAppropriateContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return true;
        }
    }
}