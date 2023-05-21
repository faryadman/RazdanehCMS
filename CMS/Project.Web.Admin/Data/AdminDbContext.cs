using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Web.Admin.Models;

namespace Project.Web.Admin.Data
{
    public partial class AdminDbContext :
        IdentityDbContext<User, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserRole>(entity =>
            {
                entity.HasOne(userRole => userRole.Role)
                        .WithMany(role => role.UserRoles)
                        .HasForeignKey(userRole => userRole.RoleId);

                entity.HasOne(userRole => userRole.User)
                       .WithMany(user => user.UserRoles)
                       .HasForeignKey(userRole => userRole.UserId);
            });
            builder.Entity<RoleClaim>(entity =>
            {
                entity.HasOne(roleClaim => roleClaim.Role)
                   .WithMany(role => role.RoleClaims)
                   .HasForeignKey(roleClaim => roleClaim.RoleId);
            });

            builder.Entity<UserClaim>(entity =>
            {
                entity.HasOne(userClaim => userClaim.User)
                   .WithMany(user => user.UserClaims)
                   .HasForeignKey(userClaim => userClaim.UserId);
            });

            builder.Entity<UserLogin>(entity =>
            {
                entity.HasOne(userLogin => userLogin.User)
                   .WithMany(user => user.Logins)
                   .HasForeignKey(userLogin => userLogin.UserId);
            });
            builder.Entity<UserToken>(entity =>
            {
                entity.HasOne(userToken => userToken.User)
                   .WithMany(user => user.UserTokens)
                   .HasForeignKey(userToken => userToken.UserId);
            });

            var adminName = "admin";

            var user = new User
            {
                Id = "4a9bbe2a-00df-4e89-9116-bee610e6b41b",
                Email = $"{adminName}@site.come",
                NormalizedEmail = $"{adminName}@site.come".Normalize(),
                EmailConfirmed = true,
                UserName = adminName,
                NormalizedUserName = adminName.Normalize(),
                PasswordHash = "AQAAAAEAACcQAAAAENYv8QSjav8XjH8XjYnE9b0/5pFUQDcP+HZEwYdk7g7vOqfFrHPuA+RKD/8SUWwvQA==", // Z!l10w
                ConcurrencyStamp = "a66289aa-b49d-402a-b224-885702748aa6",
                SecurityStamp = "7a501d67-f0a1-4f34-b36d-726e95282f7e"
            };
            var role = new Role
            {
                Id = "2e5cf5ba-9aa6-48a3-b7ce-a4ec8a478ce4",
                Name = adminName,
                NormalizedName = adminName.Normalize(),
                ConcurrencyStamp = "ecf978b4-9d5d-47b1-94eb-225d87f9ed2e",
                Description = adminName,
            };

            builder.Entity<User>().HasData(user);

            builder.Entity<Role>().HasData(role);

            builder.Entity<UserRole>().HasData(new UserRole
            {
                //Role = role,
                RoleId = role.Id,
                UserId = user.Id,
                //User = user,
            });

            //builder.Entity<UserRole>().HasData(new UserRole
            //{
            //    Role = role,
            //    User = user,
            //    RoleId = "2e5cf5ba-9aa6-48a3-b7ce-a4ec8a478ce4",
            //    UserId = "4a9bbe2a-00df-4e89-9116-bee610e6b41b"
            //});
        }
    }
}
