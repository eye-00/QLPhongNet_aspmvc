using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class ServiceUsage
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int Quantity { get; set; }
        public DateTime UsageTime { get; set; }
        public decimal? TotalPrice { get; set; }

        public User User { get; set; }
        public Service Service { get; set; }
    }
}
