using Microsoft.AspNetCore.Mvc.Rendering;
using Project.Application.DTOs.Datatable.Base;
using Project.Web.Admin.Models;
using Project.Web.Admin.ViewModels;

namespace Project.Web.Admin.Interfaces
{
    public interface IRoleRepository
    {
        Task AddOrUpdateRoleClaimsAsync(string roleId, string roleClaimType, IList<string> selectedRoleClaimValues);
        Task AddRole(RoleVM viewModel);
        Task<DatatableResponse<RoleVM>> Datatable(FiltersFromRequestDataTable filtersFromRequest);
        Task DeleteRole(string roleId);
        Task<Role> FindRoleIncludeRoleClaimsAsync(string roleId);
        Task<IEnumerable<SelectListItem>> RoleListForDropdown();
        Task<List<RoleVM>> RolesList();
        Task UpdateRole(RoleVM viewModel);
    }
}
