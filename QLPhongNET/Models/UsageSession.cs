using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class UsageSession
    {
        public UsageSession()
        {
            StartTime = DateTime.Now;
        }

        [Key]
        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ComputerID { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
        public decimal? TotalCost { get; set; }

        public int? DailyRevenueID { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("ComputerID")]
        public virtual Computer? Computer { get; set; }

        [ForeignKey("DailyRevenueID")]
        public virtual DailyRevenue? DailyRevenue { get; set; }
    }
}
