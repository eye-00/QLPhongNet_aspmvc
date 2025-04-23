using System.ComponentModel.DataAnnotations;

namespace QLPhongNET.Models
{
    public class UserStatisticsViewModel
    {
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Số phiên sử dụng")]
        public int TotalSessions { get; set; }

        [Display(Name = "Tổng giờ sử dụng")]
        public double TotalHours { get; set; }

        [Display(Name = "Tổng chi tiêu")]
        public decimal TotalSpent { get; set; }
    }
} 