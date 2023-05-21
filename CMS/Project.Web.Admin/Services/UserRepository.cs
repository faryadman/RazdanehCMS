using Microsoft.AspNetCore.Identity;
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
    public class UserRepository : IUserRepository
    {
        private readonly AdminDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;

        public UserRepository(AdminDbContext db, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<DatatableResponse<UserVM>> Datatable(FiltersFromRequestDataTable filtersFromRequest)
        {
            var data = _db.Users
                .Where(w => w.UserName != "admin")
                .Include(i => i.UserRoles)
                .ThenInclude(r => r.Role)
                .AsQueryable();

            var totalRecords = await data.CountAsync();

            if (!string.IsNullOrWhiteSpace(filtersFromRequest.SearchValue))
            {
                data = data.Where(w =>
                    w.UserName.ToLower().Contains(filtersFromRequest.SearchValue.NormalizeText()) ||
                    w.Email.ToLower().Contains(filtersFromRequest.SearchValue.NormalizeText())
                );
            }

            return new DatatableResponse<UserVM>
            {
                SEcho = filtersFromRequest.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = await data.CountAsync(),
                Data = await data
                        .DatatablePaginate(filtersFromRequest.Start, filtersFromRequest.Length)
                        .Select(s => new UserVM
                        {
                            Id = s.Id,
                            Email = s.Email,
                            UserName = s.UserName,
                            PhoneNumber = s.PhoneNumber,
                            RoleIds = s.UserRoles.Select(s => s.Role.Id).ToList(),
                            RoleName = s.UserRoles.Select(s => s.Role.Name).ToList()
                        })
                        .ToListAsync()
            };

        }

        public async Task AddUser(UserVM viewModel)
        {
            if (await IsUserExist(viewModel))
            {
                throw new ValidationException("این اطلاعات کاربری از قبل وجود دارد");
            }

            var model = new User
            {
                Email = viewModel.Email,
                UserName = viewModel.UserName,
                PhoneNumber = viewModel.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(model, viewModel.Password);

            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(s => s.Description).ToList());
            }
        }

        public async Task UpdateUser(UserVM viewModel)
        {
            var user = await _db.Users.Where(u => u.Id == viewModel.Id).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotFoundException("کاربر یافت نشد");
            }

            user.UserName = viewModel.UserName;
            user.NormalizedUserName = viewModel.UserName.Normalize();
            user.Email = viewModel.Email;
            user.NormalizedEmail = viewModel.Email.Normalize();
            user.PhoneNumber = viewModel.PhoneNumber;

            await _db.SaveChangesAsync();

            if (string.IsNullOrWhiteSpace(viewModel.Password) == false)
            {
                var updatedUser = await _userManager.FindByIdAsync(viewModel.Id);

                var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(updatedUser);
                var updatePassword = await _userManager.ResetPasswordAsync(updatedUser, resetPasswordToken, viewModel.Password);

                if (!updatePassword.Succeeded)
                {
                    throw new ValidationException(updatePassword.Errors.Select(s => s.Description).ToList());
                }
            }
        }

        public async Task DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("کاربر یافت نشد");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(s => s.Description).ToList());
            }
        }

        public async Task EditRoles(EditUserRolesVM viewModel)
        {
            var user = await _db.Users.Where(u => u.Id == viewModel.Id).Include(u => u.UserRoles).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotFoundException("کاربر یافت نشد");
            }

            var currentUserRoleIds = user.UserRoles.Select(x => x.RoleId).ToList();

            if (viewModel.RoleIds == null)
            {
                viewModel.RoleIds = new List<string>();
            }

            var newRolesToAdd = viewModel.RoleIds.Except(currentUserRoleIds).ToList();
            foreach (var roleId in newRolesToAdd)
            {
                user.UserRoles.Add(new UserRole { RoleId = roleId, UserId = user.Id });
            }

            var removedRoles = currentUserRoleIds.Except(viewModel.RoleIds).ToList();
            foreach (var roleId in removedRoles)
            {
                var userRole = user.UserRoles.SingleOrDefault(ur => ur.RoleId == roleId);
                if (userRole != null)
                {
                    user.UserRoles.Remove(userRole);
                }
            }

            await _userManager.UpdateAsync(user);
        }

        private async Task<bool> IsUserExist(UserVM viewModel)
        {
            return await _userManager.FindByNameAsync(viewModel.UserName) != null
                || await _userManager.FindByEmailAsync(viewModel.Email) != null;
        }
    }
}
