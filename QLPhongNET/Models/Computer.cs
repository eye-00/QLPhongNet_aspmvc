using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class Computer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

        public int CategoryID { get; set; }
        public ComputerCategory Category { get; set; }

        public ICollection<UsageSession> UsageSessions { get; set; }
    }
}
