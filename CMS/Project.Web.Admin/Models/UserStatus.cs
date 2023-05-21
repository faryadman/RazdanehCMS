using System.ComponentModel.DataAnnotations;

namespace Project.Web.Admin.Models
{
    public enum UserStatus
    {
        [Display(Name = "فعال")]
        Active,
        [Display(Name = "غیرفعال")]
        Blocked,
        [Display(Name = "در انتظار تایید ادمین")]
        AwaitingApproval,
        [Display(Name = "محدود شده")]
        Limited,
        [Display(Name = "کاربر جدید")]
        NewUser,
        [Display(Name = "احراز هویت رد شده")]
        FailedRequest
    }
}
