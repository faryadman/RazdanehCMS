using Microsoft.AspNetCore.Identity;

namespace Project.Web.Admin.Models
{
    public class UserToken : IdentityUserToken<string>
    {
        public virtual User User { get; set; }
    }
}
