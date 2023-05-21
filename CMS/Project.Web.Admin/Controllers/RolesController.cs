using DNTCommon.Web.Core;
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
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("نقش ها")]
    public class RolesController : Controller
    {

        private readonly IRoleRepository _roleRepository;
        private readonly IMvcActionsDiscoveryService _mvcActionsDiscoveryService;
        public RolesController(IRoleRepository roleRepository, IMvcActionsDiscoveryService mvcActionsDiscoveryService)
        {
            _roleRepository = roleRepository;
            _mvcActionsDiscoveryService = mvcActionsDiscoveryService;
        }

        [DisplayName("لیست نقش ها")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست نقش ها";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData()
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _roleRepository.Datatable(filters);
            return Json(res);
        }

        [DisplayName("ثبت و ویرایش")]
        [HttpPost]
        public async Task<JsonResult> Upsert(RoleVM input)
        {
            if (string.IsNullOrWhiteSpace(input.Id))
            {
                await _roleRepository.AddRole(input);
                return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
            }
            else
            {
                await _roleRepository.UpdateRole(input);
                return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
            }
        }

        [DisplayName("حذف نقش")]
        [Route("delete/{id}")]
        public async Task<JsonResult> Delete(string id)
        {
            await _roleRepository.DeleteRole(id);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }

        [DisplayName("مدیریت نقش")]
        public async Task<IActionResult> Manage(string id)
        {
            ViewBag.Title = "مدیریت نقش";
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var role = await _roleRepository.FindRoleIncludeRoleClaimsAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var securedControllerActions = _mvcActionsDiscoveryService
                .GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);

            var model = new DynamicRoleClaimsManagerViewModel
            {
                SecuredControllerActions = securedControllerActions,
                RoleIncludeRoleClaims = role
            };

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Manage(DynamicRoleClaimsManagerViewModel model)
        {
            await _roleRepository.AddOrUpdateRoleClaimsAsync(
                roleId: model.RoleId,
                roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
                selectedRoleClaimValues: model.ActionIds);

            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
    }
}
