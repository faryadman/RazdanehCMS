using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Entities
{
    [Index(nameof(ApiLog.Id), IsUnique = true)]
    public class ApiLog : BaseEntity
    {
        [ForeignKey("AppSettingId")]
        public virtual AppSetting AppSetting { get; set; }
        public int AppSettingId { get; set; }
        [ForeignKey("ServerId")]
        public virtual Server Server { get; set; }
        public int ServerId { get; set; }
    }
}
