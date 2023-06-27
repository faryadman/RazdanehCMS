using Project.Domain.Entities.Base;

namespace Project.Domain.Entities
{
    public class SaveIP : BaseEntity
    {
        public string Ip { get; set; }
        public string Tcp { get; set; }
        public string UserAgent { get; set; }
    }
}
