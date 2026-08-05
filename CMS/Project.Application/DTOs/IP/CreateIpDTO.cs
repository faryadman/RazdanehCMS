namespace Project.Application.DTOs.IP
{
    public class CreateIpDTO
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public string Tcp { get; set; }
        public string UserAgent { get; set; }
    }
}
