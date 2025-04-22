using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class LoginSession
    {
        [Key]
        public int ID { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 