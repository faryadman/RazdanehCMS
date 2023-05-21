namespace Project.Web.Admin.ViewModels
{
    public class UserRoleVM
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public UserVM User { get; set; }
        public RoleVM Role { get; set; }
    }
}
