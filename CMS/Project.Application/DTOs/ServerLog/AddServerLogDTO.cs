using Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Application.DTOs.ServerLog
{
    public class AddServerLogDTO
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
        public string Operator { get; set; }
        public string ApiRoute { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
