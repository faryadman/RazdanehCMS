using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    [Index(nameof(AppSetting.ApiRoute), IsUnique = true)]
    public class AppSetting : BaseEntity
    {
        [StringLength(256)]
        public string ApiRoute { get; set; }
        [StringLength(256)]
        public string GroupsThatAppIsJoinedIn { get; set; }
        [StringLength(256)]
        public string AppTitle { get; set; }
        [StringLength(256)] public string InterAC { get; set; }
        [StringLength(256)] public string InterAD { get; set; }
        [StringLength(256)] public string InterSP { get; set; }
        [StringLength(256)] public string InterPreOne { get; set; }
        [StringLength(256)] public string InterPreTwo { get; set; }
        public int SplashTimeOut { get; set; }
        public bool IsAdmobActive { get; set; }
        public bool IsSplashActive { get; set; }
        [StringLength(256)] public string StoreVersion { get; set; }
        public bool IsUpdateForce { get; set; }
        public int ResponseTimeout { get; set; }
        public int RetryNumber { get; set; }
        [StringLength(256)] public string ShareUi { get; set; }
        public bool CheckIp { get; set; }
        [StringLength(256)] public string Url { get; set; }
        [StringLength(256)] public string AppId { get; set; }
        [StringLength(256)] public string BannerId { get; set; }
        public bool SendRandomServer { get; set; }
        public bool IsLogAllowed { get; set; }
        public bool IsAdServerAllowed { get; set; }
        public long TimerCounter { get; set; }
        public bool?  IsGdprAllowed { get; set; }
        public bool?  IsAcAllowed { get; set; }
        public bool?  IsSpAllowed { get; set; }
        public bool?  IsTimeToStopAllowed { get; set; }
        public int? TimeToStop { get; set; }

    }
}