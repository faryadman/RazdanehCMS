using Project.Domain.Entities.Base;

namespace Project.Domain.Entities
{
    public class Server : BaseEntity
    {
        public string ServerName { get; set; }
        public string Location { get; set; }
        public string Ip { get; set; }
        public string Config { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        //public string CurrentDomainValue { get; set; }
        public Group Group { get; set; }
        public int GroupId { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsAd { get; set; }
        public string CurrentDomainValue { get; set; }
        public bool IsNewDomain { get; set; }
        public bool IsForIrancell { get; set; }
        public bool IsForHamraheAvval { get; set; }
        public ICollection<ServerLog> Logs { get; set; }
        public DateTime DomainDateTime { get; set; } = DateTime.UtcNow;
    }
}
