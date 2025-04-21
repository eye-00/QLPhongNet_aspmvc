using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class DailyRevenue
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Ngày báo cáo")]
        [DataType(DataType.Date)]
        public DateTime ReportDate { get; set; }

        [Required]
        [Display(Name = "Doanh thu sử dụng máy")]
        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalUsageRevenue { get; set; }

        [Required]
        [Display(Name = "Doanh thu nạp tiền")]
        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalRecharge { get; set; }

        [Required]
        [Display(Name = "Doanh thu dịch vụ")]
        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalServiceRevenue { get; set; }

        public virtual ICollection<UsageSession> UsageSessions { get; set; } = new List<UsageSession>();
        public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();
    }
}
