using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Responses;
using Project.Web.Admin.Models;
using Project.Web.Admin.ViewModels;

namespace Project.Web.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index(string returnUrl)
        {
            returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

            if (_signInManager.IsSignedIn(User))
            {
                return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("index");
        }

        public IActionResult Login(string returnUrl)
        {
            returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

            if (_signInManager.IsSignedIn(User))
            {
                return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("index");
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel input)
        {
            var ReturnUrl = string.IsNullOrWhiteSpace(input.ReturnUrl) ? "/" : input.ReturnUrl;
            var find = await _userManager.FindByNameAsync(input.Username);
            if (find == null)
            {
                return new Response<string>(ResponseStatus.BadRequest, message: "کاربر یافت نشد").ToJsonResult();
            }

            var result = await _signInManager.PasswordSignInAsync(input.Username, input.Password, input.RememberMe, lockoutOnFailure: false);

            return result.Succeeded
                ? new Response<string>(ResponseStatus.Succeed, data: ReturnUrl).ToJsonResult()
                : new Response<string>(ResponseStatus.BadRequest, message: "کلمه عبور صحیح نمی‌باشد").ToJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect(Url.Content("/account"));
        }
        public IActionResult AccessDenied()
        {
            ViewBag.ReturnUrl = "/";
            return View();
        }

    }
}
