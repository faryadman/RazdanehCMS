using Project.Application.DTOs.Base;
using Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.ServerLog
{
    public class ServerLogDTO : BaseDTO
    {
        [Required]
        public int ServerId { get; set; }
        [Required]
        public string UserId { get; set; }
        public string Ip { get; set; }
        public string Isp { get; set; }
        public string City { get; set; }
        public string Org { get; set; }
        public string Country { get; set; }
        public Operator Operator { get; set; }
        public string OperatorName { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
