using System.ComponentModel.DataAnnotations;

namespace QLPhongNET.Models
{
    public class DailyRevenue
    {
        public int ID { get; set; }
        public DateTime ReportDate { get; set; }
        public decimal TotalUsageRevenue { get; set; }
        public decimal TotalRecharge { get; set; }
        public decimal TotalServiceRevenue { get; set; }
    }
}
