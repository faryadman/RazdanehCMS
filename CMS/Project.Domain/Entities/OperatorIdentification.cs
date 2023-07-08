using Project.Domain.Entities.Base;
using Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    public class OperatorIdentification : BaseEntity
    {
        [StringLength(256)]
        public string Text { get; set; }
        public Operator Operator { get; set; }
        public bool IsIsp { get; set; }
    }
}
