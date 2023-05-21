using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.AppSetting
{
    public class CreateAppSettingDTO
    {
        [Required]
        public string ApiRoute { get; set; }
        [Required]
        public string StoreVersion { get; set; }
        [Required]
        public string AppTitle { get; set; }
    }
}
