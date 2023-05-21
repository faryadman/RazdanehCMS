using Project.Application.DTOs.Group;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.Server
{
    public class CreateServerDTO
    {
        [Required]
        public string ServerName { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Ip { get; set; }
        [Required]
        public string Config { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public int GroupId { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsForIrancell { get; set; }
        public bool IsForHamraheAvval { get; set; }
        public bool IsAd { get; set; }
    }
}
