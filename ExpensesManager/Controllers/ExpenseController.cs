using ExpensesManager.Core.Services;
using ExpensesManager.Database.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExpensesManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _service;

        public ExpenseController(IExpenseService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Get([FromQuery] ExpenseQueryParameters parameters)
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
                return StatusCode(500, "An error occurred while retrieving expenses");
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
                var expense = await _service.GetByIdAsync(id);
                if (expense == null)
                {
                    return NotFound($"Expense with ID {id} not found");
                }
                return Ok(expense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the expense");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Post([FromBody] CreateExpenseDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Expense data cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.AddAsync(dto);
                if (result == null)
                {
                    return BadRequest("Failed to create expense. Please verify UserId and CategoryId are valid");
                }

                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (use your logging framework)
                return StatusCode(500, "An error occurred while creating the expense");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Put(
            [Range(1, int.MaxValue, ErrorMessage = "ID must be a positive integer")] int id,
            [FromBody] CreateExpenseDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Expense data cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (!updated)
                {
                    return BadRequest("Failed to update expense. Please verify the expense exists and UserId/CategoryId are valid");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the expense");
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
                    return NotFound($"Expense with ID {id} not found or could not be deleted");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the expense");
            }
        }
    }
}