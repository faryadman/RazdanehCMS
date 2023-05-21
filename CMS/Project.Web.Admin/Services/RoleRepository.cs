using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.Exceptions;
using Project.Application.Extensions;
using Project.Web.Admin.Data;
using Project.Web.Admin.Interfaces;
using Project.Web.Admin.Models;
using Project.Web.Admin.ViewModels;

namespace Project.Web.Admin.Services
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AdminDbContext _db;
        private readonly RoleManager<Role> _roleManager;

        public RoleRepository(AdminDbContext db, RoleManager<Role> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
        }

        public async Task AddRole(RoleVM viewModel)
        {
            if (IsExistRole(viewModel.Name))
            {
                throw new ValidationException("این نقش از قبل وجود دارد");
            }

            var model = new Role
            {
                Name = viewModel.Name,
                NormalizedName = viewModel.Name.Normalize(),
                Description = viewModel.Description
            };

            var result = await _roleManager.CreateAsync(model);
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(s => s.Description).ToList());
            }
        }

        public async Task UpdateRole(RoleVM viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Id))
            {
                throw new ValidationException("شناسه نقش را وارد کنید");
            }
            if (IsExistRole(viewModel.Id, viewModel.Name))
            {
                throw new ValidationException("نقشی با این نام وجود دارد");
            }
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == viewModel.Id);
            if (role == null)
            {
                throw new NotFoundException();
            }
            role.Name = viewModel.Name;
            role.Description = viewModel.Description;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteRole(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
            {
                throw new ValidationException("شناسه نقش را وارد کنید");
            }
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                throw new NotFoundException("نقش یافت نشد");
            }
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(s => s.Description).ToList());
            }
        }

        public async Task<Role> FindRoleIncludeRoleClaimsAsync(string roleId)
        {
            return await _db.Roles.Include(x => x.RoleClaims).FirstOrDefaultAsync(x => x.Id == roleId);
        }

        public async Task AddOrUpdateRoleClaimsAsync(string roleId, string roleClaimType, IList<string> selectedRoleClaimValues)
        {
            var role = await FindRoleIncludeRoleClaimsAsync(roleId);
            if (role == null)
            {
                throw new NotFoundException("نقش مورد نظر یافت نشد.");
            }

            var currentRoleClaimValues = role.RoleClaims.Where(roleClaim => roleClaim.ClaimType == roleClaimType)
                                                    .Select(roleClaim => roleClaim.ClaimValue)
                                                    .ToList();

            if (selectedRoleClaimValues == null)
            {
                selectedRoleClaimValues = new List<string>();
            }
            var newClaimValuesToAdd = selectedRoleClaimValues.Except(currentRoleClaimValues).ToList();
            foreach (var claimValue in newClaimValuesToAdd)
            {
                role.RoleClaims.Add(new RoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = roleClaimType,
                    ClaimValue = claimValue
                });
            }

            var removedClaimValues = currentRoleClaimValues.Except(selectedRoleClaimValues).ToList();
            foreach (var claimValue in removedClaimValues)
            {
                var roleClaim = role.RoleClaims.SingleOrDefault(rc => rc.ClaimValue == claimValue &&
                                                                  rc.ClaimType == roleClaimType);
                if (roleClaim != null)
                {
                    role.RoleClaims.Remove(roleClaim);
                }
            }

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(s => s.Description).ToList());
            }
        }

        public async Task<IEnumerable<SelectListItem>> RoleListForDropdown()
        {
            return await _db.Roles
                .Select(i => new SelectListItem()
                {
                    Text = i.Name,
                    Value = i.Id
                }).ToListAsync();
        }

        public async Task<List<RoleVM>> RolesList()
        {
            return await _db.Roles
                .Select(s => new RoleVM
                {
                    Id = s.Id,
                    Description = s.Description,
                    Name = s.Name
                })
                .ToListAsync();
        }

        public async Task<DatatableResponse<RoleVM>> Datatable(FiltersFromRequestDataTable filtersFromRequest)
        {
            var data = _db.Roles
                .Include(i => i.UserRoles)
                .ThenInclude(i2 => i2.User)
                .Where(w => w.Name != "admin")
                .AsQueryable();

            var totalRecords = await data.CountAsync();

            if (!string.IsNullOrWhiteSpace(filtersFromRequest.SearchValue))
            {
                data = data.Where(w =>
                    w.Name.ToLower().Contains(filtersFromRequest.SearchValue.NormalizeText())
                );
            }

            return new DatatableResponse<RoleVM>
            {
                SEcho = filtersFromRequest.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = await data.CountAsync(),
                Data = await data
                        .DatatablePaginate(filtersFromRequest.Start, filtersFromRequest.Length)
                        .Select(s => new RoleVM
                        {
                            Description = s.Description,
                            Name = s.Name,
                            Id = s.Id
                        })
                        .ToListAsync()
            };
        }

        private bool IsExistRole(string roleName)
        {
            return _db.Roles.Any(r => r.Name.ToLower().Trim().Equals(roleName.ToLower().Trim()));
        }

        private bool IsExistRole(string roleId, string roleName)
        {
            return _db.Roles.Any(r => r.Id != roleId && r.Name.ToLower().Trim().Equals(roleName.ToLower().Trim()));
        }
    }
}
