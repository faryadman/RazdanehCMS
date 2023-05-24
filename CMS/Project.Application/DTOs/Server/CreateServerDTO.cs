using System.ComponentModel.DataAnnotations;

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
        public string CurrentDomainValue { get; set; }
        public int GroupId { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsForIrancell { get; set; }
        public bool IsForHamraheAvval { get; set; }
        public bool IsAd { get; set; }

    }
}
