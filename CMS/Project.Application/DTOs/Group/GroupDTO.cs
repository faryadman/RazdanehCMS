using Project.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.Group
{
    public class GroupDTO : BaseDTO
    {
        public string Title { get; set; }
        public bool IsAd { get; set; }
    }
}
