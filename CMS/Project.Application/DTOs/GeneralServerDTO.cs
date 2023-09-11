using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.Server;

namespace Project.Application.DTOs
{
    public class GeneralServerDTO
    {
        public ServerDTO ServerDTO { get; set; }
        public AppSettingDTO AppSettingDTO { get; set; }
    }
}
