using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Products
{
    public class FarmDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
    }

    public class CreateFarmRequest
    {
        [Required(ErrorMessage = "Farm name is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Farm name must be 2-150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ContactInfo { get; set; }
    }

    public class UpdateFarmRequest : CreateFarmRequest { }
}