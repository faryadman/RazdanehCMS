using Project.Domain.Entities.Base;

namespace Project.Domain.Entities
{
    public class Domain : BaseEntity
    {
        public string DomainName { get; set; }
        public string FileName { get; set; }
    }
}