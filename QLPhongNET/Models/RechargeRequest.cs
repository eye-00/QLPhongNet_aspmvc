using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public enum RechargeStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class RechargeRequest
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [Required]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền nạp tối thiểu là 1,000 VNĐ")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime RequestTime { get; set; } = DateTime.Now;

        public DateTime? ProcessedTime { get; set; }

        [Required]
        public RechargeStatus Status { get; set; } = RechargeStatus.Pending;

        public string? Note { get; set; }

        public int? DailyRevenueID { get; set; }

        [ForeignKey("DailyRevenueID")]
        public virtual DailyRevenue? DailyRevenue { get; set; }
    }
} 