using System.Collections.Generic;

namespace GreenBasket.Application.DTOs.Admin
{
    public class UserWithRolesDTO
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
