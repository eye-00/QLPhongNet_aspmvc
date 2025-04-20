using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class UsageSession
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int ComputerID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? TotalCost { get; set; }

        public User User { get; set; }
        public Computer Computer { get; set; }
    }
}
