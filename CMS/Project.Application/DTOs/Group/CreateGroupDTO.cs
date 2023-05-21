using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.Group
{
    public class CreateGroupDTO
    {
        [Required]
        public string Title { get; set; }
        public bool IsAd { get; set; }
    }
}
