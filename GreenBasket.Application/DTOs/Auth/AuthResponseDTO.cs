using System.Collections.Generic;

namespace GreenBasket.Application.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string Token { get; set; } = string.Empty;
    }
}