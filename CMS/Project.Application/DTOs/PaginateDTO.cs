using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs
{
    public class PaginateDTO
    {
        public int PageNumber { get; set; }
        public int PageDataCount { get; set; }
        public int DataCount { get; set; }
    }
}
