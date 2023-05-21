using Project.Application.DTOs.Base;
using Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.OperatorIdentification
{
    public class OperatorIdentificationDTO : BaseDTO
    {
        public string Text { get; set; }
        public Operator Operator { get; set; }
        public string OperatorName { get; set; }
        public bool IsIsp { get; set; }
    }
}
