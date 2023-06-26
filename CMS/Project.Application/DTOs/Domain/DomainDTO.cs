using Project.Application.DTOs.Base;

namespace Project.Application.DTOs.Domain
{
    public class DomainDTO : BaseDTO
    {
        public string DomainName { get; set; }
        public string FileName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }
}
