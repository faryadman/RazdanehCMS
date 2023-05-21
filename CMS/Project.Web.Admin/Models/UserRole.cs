using Microsoft.AspNetCore.Identity;

namespace Project.Web.Admin.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public virtual User User { get; set; }

        public virtual Role Role { get; set; }
    }
}
