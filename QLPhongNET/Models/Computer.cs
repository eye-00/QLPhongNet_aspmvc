using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public enum ComputerStatus
    {
        Available,
        [Display(Name = "In Use")]
        InUse,
        Maintenance
    }

    public class Computer
    {
        public Computer()
        {
            Name = string.Empty;
            Status = ComputerStatus.Available;
            UsageSessions = new HashSet<UsageSession>();
        }

        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên máy")]
        [StringLength(30, ErrorMessage = "Tên máy không được vượt quá 30 ký tự")]
        [Display(Name = "Tên máy")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ComputerStatus Status { get; set; }

        [Required]
        public int CatID { get; set; }

        [ForeignKey("CatID")]
        public virtual ComputerCategory? Category { get; set; }

        public virtual ICollection<UsageSession> UsageSessions { get; set; } = new List<UsageSession>();
    }
}
