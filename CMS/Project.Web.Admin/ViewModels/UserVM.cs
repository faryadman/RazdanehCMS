namespace Project.Web.Admin.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public List<string> RoleName { get; set; }
        public IEnumerable<string> RoleIds { get; set; }
    }
}
