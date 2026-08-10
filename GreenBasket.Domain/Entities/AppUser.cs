using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Net;

namespace GreenBasket.Domain.Entities
{
    // Kế thừa IdentityUser để lấy sẵn các trường Id, Email, PhoneNumber, PasswordHash... [cite: 378]
    public class AppUser : IdentityUser
    {
        // Thêm các trường mở rộng không có sẵn trong IdentityUser [cite: 379]
        public string FullName { get; set; } = string.Empty;

        // Navigation property: Thể hiện quan hệ 1-N (1 User có nhiều Address) [cite: 366]
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

        public virtual Cart? Cart { get; set; }
    }
}