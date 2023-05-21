using Project.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.ApiLog
{
    public class ApiLogDTO : BaseDTO
    {
        public int AppSettingId { get; set; }
        public int ServerId { get; set; }
    }
}
