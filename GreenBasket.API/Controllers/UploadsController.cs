using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenBasket.API.Controllers
{
    [Route("api/admin/uploads")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // POST: api/admin/uploads/image
        // Nhận multipart/form-data với field "file", lưu vào wwwroot/uploads/products,
        // trả về URL tương đối để lưu vào Product.ImageUrl.
        [HttpPost("image")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return BadRequest(new { message = "File size must not exceed 5MB." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only .jpg, .jpeg, .png, .webp files are allowed." });
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/products/{fileName}";
            return Ok(new { url });
        }
    }
}
