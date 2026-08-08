using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenBasket.API.Controllers
{
    [Route("api/admin/farms")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class FarmsController : ControllerBase
    {
        private readonly IFarmService _farmService;

        public FarmsController(IFarmService farmService)
        {
            _farmService = farmService;
        }

        // GET: api/admin/farms
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var farms = await _farmService.GetAllAsync();
            return Ok(farms);
        }

        // GET: api/admin/farms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var farm = await _farmService.GetByIdAsync(id);
            if (farm == null) return NotFound(new { Message = $"Can't find the farm with the ID = {id}." });
            return Ok(farm);
        }

        // POST: api/admin/farms
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFarmRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var farm = await _farmService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = farm.Id }, farm);
        }

        // PUT: api/admin/farms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFarmRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _farmService.UpdateAsync(id, request);
            if (!success) return NotFound(new { Message = $"Can't find the farm with the ID = {id}." });

            return NoContent();
        }

        // DELETE: api/admin/farms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _farmService.DeleteAsync(id);
            if (!success)
            {
                return Conflict(new { Message = $"Can't delete this farm with ID = {id} — this farm has existing batch history." });
            }

            return NoContent();
        }
    }
}