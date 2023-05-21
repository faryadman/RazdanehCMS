using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Group;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IServerService _serverService;

        public GroupsController(IGroupService groupService, IServerService serverService)
        {
            _groupService = groupService;
            _serverService = serverService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> List(bool isAd)
        {
            var data = await _groupService.GetFiltered(isAd);
            return Json(data);
        } 
        public IActionResult AdIndex()
        {
            return View();
        }
        public async Task<IActionResult> Create(CreateGroupDTO input)
        {
            await _groupService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }

        public async Task<IActionResult> CreateAd(CreateGroupDTO input)
        {
            input.IsAd = true;
            await _groupService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Edit(EditGroupDTO input)
        {
            await _groupService.Edit(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> EditAd(EditGroupDTO input)
        {
            await _groupService.Edit(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _groupService.Delete(id);
            await _serverService.DeleteByGroupId(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> MassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await _groupService.Delete(int.Parse(item));
                await _serverService.DeleteByGroupId(int.Parse(item));
            }
            return Json(new { status = "1", message = "done successfully" });
        }

    }
}
