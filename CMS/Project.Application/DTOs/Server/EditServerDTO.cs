using System.ComponentModel.DataAnnotations;

namespace Project.Application.DTOs.Server
{
    public class EditServerDTO
    {
        public int ItemId { get; set; }
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
        [Required]
        public string CurrentDomainValue { get; set; }
        public bool IsForIrancell { get; set; }
        public bool IsForHamraheAvval { get; set; }
        public bool IsNewDomain { get; set; }
        public DateTime DomainDateTime { get; set; }
        [StringLength(256)]
        public string? ZoneId { get; set; }
        public DateTime? ZoneIdLastSynced { get; set; }

    }
}
