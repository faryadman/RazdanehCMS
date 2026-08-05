using Project.Application.DTOs.Base;
using Project.Domain.Enums;

namespace Project.Application.DTOs.ApiLog
{
    public class ApiLogDTO : BaseDTO
    {
        public int AppSettingId { get; set; }
        public int ServerId { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
