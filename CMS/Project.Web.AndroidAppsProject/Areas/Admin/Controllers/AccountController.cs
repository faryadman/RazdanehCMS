using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Responses;
using Project.Domain.Entities;
using Project.Persistence;
using Project.Web.AndroidAppsProject.Areas.Admin.ViewModels;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return Redirect("/admin/apps");
            }
            return View("index");
        }

        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return Redirect("/admin/apps");
            }
            return View("index");
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel input)
        {
            var returnUrl = "/admin/apps";
            var find = await _userManager.FindByNameAsync(input.Username);
            if (find == null)
            {
                return new Response<string>(ResponseStatus.BadRequest, message: "نام کاربری یا کلمه عبور نادرست می باشد").ToJsonResult();
            }

            var result = await _signInManager.PasswordSignInAsync(input.Username, input.Password, true, lockoutOnFailure: false);

            return result.Succeeded
                ? new Response<string>(ResponseStatus.Succeed, data: returnUrl).ToJsonResult()
                : new Response<string>(ResponseStatus.BadRequest, message: "نام کاربری یا کلمه عبور نادرست می باشد").ToJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect(Url.Content("/admin/account"));
        }
    }
}
//var user = new Domain.Entities.User
//{
//    Email = "admin@gmail.com",
//    EmailConfirmed = true,
//    LockoutEnabled = false,
//    NormalizedEmail = "admin@gmail.com".Normalize(),
//    Status = Domain.Enums.UserStatus.Active,
//    UserName = "admin",
//    NormalizedUserName = "admin".Normalize()
//};
//await _userManager.CreateAsync(user, "Admin@123");

//await _userManager.AddToRoleAsync(user, "admin");
//await _roleManager.CreateAsync(new IdentityRole {Name = "admin" , NormalizedName = "admin".Normalize() });