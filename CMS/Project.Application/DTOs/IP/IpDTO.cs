using Project.Application.DTOs.Base;

namespace Project.Application.DTOs.IP
{
    public class IpDTO : BaseDTO
    {
        public string Ip { get; set; }
        public string Tcp { get; set; }
    }
}
