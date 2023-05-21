using Microsoft.AspNetCore.Identity;

namespace Project.Web.Admin.Models
{
    public class User : IdentityUser
    {
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<UserClaim> UserClaims { get; set; }
        public virtual ICollection<UserLogin> Logins { get; set; }
        public virtual ICollection<UserToken> UserTokens { get; set; }
    }
}
