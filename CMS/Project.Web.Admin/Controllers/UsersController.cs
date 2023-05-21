using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.Extensions;
using Project.Application.Responses;
using Project.Web.Admin.Interfaces;
using Project.Web.Admin.Models;
using Project.Web.Admin.ViewModels;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = ConstantPolicies.DynamicPermission)]
    [DisplayName("ادمین ها")]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        public UsersController(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        [DisplayName("لیست ادمین ها")]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepository.RolesList();
            ViewBag.Title = "لیست ادمین ها";
            return View(roles);
        }

        [HttpPost]
        public async Task<JsonResult> GetData()
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _userRepository.Datatable(filters);
            return Json(res);
        }

        [DisplayName("ثبت و ویرایش ادمین")]
        [HttpPost]
        public async Task<JsonResult> Upsert(UserVM input)
        {
            if (string.IsNullOrWhiteSpace(input.Id))
            {
                await _userRepository.AddUser(input);
                return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
            }
            else
            {
                await _userRepository.UpdateUser(input);
                return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
            }
        }

        [DisplayName("حذف ادمین")]
        [Route("delete/{id}")]
        public async Task<JsonResult> Delete(string id)
        {
            await _userRepository.DeleteUser(id);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }

        [DisplayName("ویرایش نقش های کاربر")]
        [HttpPost]
        public async Task<JsonResult> EditRoles(EditUserRolesVM input)
        {
            await _userRepository.EditRoles(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
    }
}
