using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.AppSetting
{
    public class AppSettingDTO
    {
        public int Id { get; set; }
        public string ApiRoute { get; set; }
        [Required]
        public string AppTitle { get; set; }
        public string InterAC { get; set; }
        public string InterAD { get; set; }
        public string InterSP { get; set; }
        public string InterPreOne { get; set; }
        public string InterPreTwo { get; set; }
        public int SplashTimeOut { get; set; }
        public bool IsAdmobActive { get; set; }
        public bool IsSplashActive { get; set; }
        [Required]
        public string StoreVersion { get; set; }
        public bool IsUpdateForce { get; set; }
        public int ResponseTimeout { get; set; }
        public int RetryNumber { get; set; }
        public string ShareUi { get; set; }
        public bool CheckIp { get; set; }
        public string Url { get; set; }
        public string AppId { get; set; }
        public string BannerId { get; set; }
        public string GroupsThatAppIsJoinedIn { get; set; }
        public string ServerApiRoute { get; set; }
        public bool SendRandomServer { get; set; }
        public bool IsLogAllowed { get; set; }

        public bool IsAdServerAllowed { get; set; }
        public long TimerCounter { get; set; }
    }
}
