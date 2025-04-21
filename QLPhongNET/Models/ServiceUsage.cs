using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class ServiceUsage
    {
        public ServiceUsage()
        {
            UsageTime = DateTime.Now;
            Quantity = 1;
        }

        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ServiceID { get; set; }

        public int? DailyRevenueID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required]
        public DateTime UsageTime { get; set; }

        public decimal? TotalPrice { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("ServiceID")]
        public virtual Service? Service { get; set; }

        [ForeignKey("DailyRevenueID")]
        public virtual DailyRevenue? DailyRevenue { get; set; }
    }
}
