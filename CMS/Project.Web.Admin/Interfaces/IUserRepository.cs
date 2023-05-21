using Project.Application.DTOs.Datatable.Base;
using Project.Web.Admin.ViewModels;

namespace Project.Web.Admin.Interfaces
{
    public interface IUserRepository
    {
        Task AddUser(UserVM viewModel);
        Task<DatatableResponse<UserVM>> Datatable(FiltersFromRequestDataTable filtersFromRequest);
        Task DeleteUser(string userId);
        Task EditRoles(EditUserRolesVM viewModel);
        Task UpdateUser(UserVM viewModel);
    }
}
