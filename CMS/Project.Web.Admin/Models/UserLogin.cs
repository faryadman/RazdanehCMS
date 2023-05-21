using Microsoft.AspNetCore.Identity;

namespace Project.Web.Admin.Models
{
    public class UserLogin : IdentityUserLogin<string>
    {
        public virtual User User { get; set; }
    }
}
