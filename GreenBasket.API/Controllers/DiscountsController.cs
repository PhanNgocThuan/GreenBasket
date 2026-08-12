using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GreenBasket.Application.DTOs.Admin;
using GreenBasket.Application.Interfaces;

namespace GreenBasket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountsController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        // GET: api/discounts/admin/all
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var discounts = await _discountService.GetAllAsync();
            return Ok(discounts);
        }

        // GET: api/discounts/admin/{id}
        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(int id)
        {
            var discount = await _discountService.GetByIdAsync(id);
            if (discount == null) return NotFound(new { message = "Discount not found." });
            return Ok(discount);
        }

        // POST: api/discounts/admin
        [HttpPost("admin")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateDiscountDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var discount = await _discountService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = discount.Id }, discount);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/discounts/admin/{id}
        [HttpPut("admin/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDiscountDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _discountService.UpdateAsync(id, request);
            if (!success) return NotFound(new { message = "Discount not found." });
            return NoContent();
        }

        // DELETE: api/discounts/admin/{id}
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _discountService.DeleteAsync(id);
                if (!success) return NotFound(new { message = "Discount not found." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // GET: api/discounts/validate/{code}
        [HttpGet("validate/{code}")]
        [Authorize]
        public async Task<IActionResult> ValidateCode(string code)
        {
            var discount = await _discountService.ValidateCodeAsync(code);
            if (discount == null)
            {
                return BadRequest(new { message = "Invalid or expired discount code." });
            }
            return Ok(discount);
        }
    }
}
