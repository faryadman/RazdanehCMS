using System.ComponentModel.DataAnnotations;

namespace Project.Application.DTOs.Domain
{
    public class CreateDomainDTO
    {
        [Required]
        public string DomainName { get; set; }
        [Required]
        public string FileName { get; set; }
    }
}
