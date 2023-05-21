using Microsoft.AspNetCore.Identity;

namespace Project.Web.Admin.Models
{
    public class RoleClaim : IdentityRoleClaim<string>
    {
        public virtual Role Role { get; set; }
    }
}
