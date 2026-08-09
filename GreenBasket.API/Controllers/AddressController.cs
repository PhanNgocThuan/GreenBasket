using GreenBasket.Application.DTOs.Address;
using GreenBasket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GreenBasket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải có Token JWT hợp lệ mới được gọi các API trong đây
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // Lấy danh sách địa chỉ của user đang đăng nhập
        [HttpGet]
        public async Task<IActionResult> GetUserAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var addresses = await _addressService.GetUserAddressesAsync(userId);
            return Ok(new { IsSuccess = true, Data = addresses });
        }

        // Thêm địa chỉ mới
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _addressService.CreateAddressAsync(userId, model);
            return Ok(new { IsSuccess = true, Message = "Address added successfully!", Data = result });
        }

        // Cập nhật địa chỉ
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _addressService.UpdateAddressAsync(id, userId, model);
            if (result == null)
                return NotFound(new { IsSuccess = false, Message = "Address not found or you do not have permission to edit it." });

            return Ok(new { IsSuccess = true, Message = "Address updated successfully!", Data = result });
        }

        // Xóa địa chỉ
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _addressService.DeleteAddressAsync(id, userId);
            if (!success)
                return NotFound(new { IsSuccess = false, Message = "Address not found or you do not have permission to delete it." });

            return Ok(new { IsSuccess = true, Message = "Address deleted successfully!" });
        }
    }
}