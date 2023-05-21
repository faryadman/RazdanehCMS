using DNTPersianUtils.Core;
using Newtonsoft.Json;

namespace Project.Application.DTOs.Base
{
    public class BaseDTO
    {
        [JsonProperty(Order = -2)]
        public int Id { get; set; }

        [JsonProperty(Order = 100)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(Order = 100)]
        public string UpdatedAtFormatted
        {
            get
            {
                return this.UpdatedAt.ToShortPersianDateTimeString(true);
            }
        }
    }
}
