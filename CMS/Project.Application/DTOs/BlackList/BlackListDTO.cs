using Project.Application.DTOs.Base;
using Project.Application.DTOs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.BlackList
{
    public class BlackListDTO : BaseDTO
    {
        public int ServerId { get; set; }
        public ServerDTO Server { get; set; }
        public string Ip { get; set; }
    }
}
