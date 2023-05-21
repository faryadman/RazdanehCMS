using Project.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.AppSetting
{
    public class MinimalAppSettingDTO : BaseDTO
    {
        public string ApiRoute { get; set; }
        public string StoreVersion { get; set; }
        public string AppTitle { get; set; }
    }
}
