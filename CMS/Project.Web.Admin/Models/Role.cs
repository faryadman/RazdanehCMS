using Microsoft.AspNetCore.Identity;

namespace Project.Web.Admin.Models
{
    public class Role : IdentityRole
    {
        public string Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<RoleClaim> RoleClaims { get; set; }
    }
}
