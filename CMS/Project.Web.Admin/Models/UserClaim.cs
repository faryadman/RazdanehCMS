using Microsoft.AspNetCore.Identity;

namespace Project.Web.Admin.Models
{
    public class UserClaim : IdentityUserClaim<string>
    {
        public virtual User User { get; set; }
    }
}
