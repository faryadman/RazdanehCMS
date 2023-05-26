using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Entities
{
    [Table("Domains", Schema = "dbo")]
    public class Domain : BaseEntity
    {
        public string DomainName { get; set; }
        public string DomainIP { get; set; }
        public string DomainType { get; set; }
    }
}