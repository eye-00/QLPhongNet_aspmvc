using System;
using System.ComponentModel.DataAnnotations;

namespace QLPhongNET.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        public User()
        {
            Username = string.Empty;
            Password = string.Empty;
            FullName = string.Empty;
            Phone = string.Empty;
            Role = UserRole.User;
            Balance = 0;
            UsageSessions = new HashSet<UsageSession>();
            ServiceUsages = new HashSet<ServiceUsage>();
            RechargeRequests = new HashSet<RechargeRequest>();
            Notifications = new HashSet<Notification>();
        }

        public int ID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(30, ErrorMessage = "Tên đăng nhập không được vượt quá 30 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu", AllowEmptyStrings = true)]
        [StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        public string Password { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(50, ErrorMessage = "Họ tên không được vượt quá 50 ký tự")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Display(Name = "Số dư")]
        public decimal Balance { get; set; }

        [Display(Name = "Vai trò")]
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public UserRole Role { get; set; }

        public virtual ICollection<UsageSession> UsageSessions { get; set; }
        public virtual ICollection<ServiceUsage> ServiceUsages { get; set; }
        public virtual ICollection<RechargeRequest> RechargeRequests { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
