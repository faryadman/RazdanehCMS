using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Entities.Base;

namespace Project.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            //var user = new User
            //{
            //    Id = "1",
            //    UserName = "admin",
            //    NormalizedUserName = "ADMIN",
            //    Email = "admin@bhsht.top",
            //    NormalizedEmail = "ADMIN@bhsht.top",
            //    EmailConfirmed = true,
            //    PasswordHash = new PasswordHasher<User>().HashPassword(null, "Admin@123")
            //};
            //modelBuilder.Entity<User>().HasData(user);
            //modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole { Name = "admin", NormalizedName = "admin".ToUpper() });

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in base.ChangeTracker.Entries<BaseEntity>()
                .Where(q => q.State == EntityState.Added || q.State == EntityState.Modified))
            {
                entry.Entity.UpdatedAt = DateTime.Now;
                entry.Entity.UpdatedBy = "SYSTEM";

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.CreatedBy = "SYSTEM";
                }
            }

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }

        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<ServerLog> ServerLogs { get; set; }
        public DbSet<BlackList> BlackLists { get; set; }
        public DbSet<ApiLog> ApiLogs { get; set; }
        public DbSet<OperatorIdentification> OperatorIdentifications { get; set; }
        public DbSet<CronJobInfo> CronJobInfos { get; set; }
        public DbSet<Domain.Entities.Domain> Domains { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<SaveIP> SaveIps { get; set; }
        public DbSet<IPConfigEntity> IPConfigEntity { get; set; }
    }
}
