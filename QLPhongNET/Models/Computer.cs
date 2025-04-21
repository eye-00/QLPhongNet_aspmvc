using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class Computer
    {
        public Computer()
        {
            Name = string.Empty;
            Status = "Available";
            UsageSessions = new HashSet<UsageSession>();
        }

        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên máy")]
        [StringLength(30, ErrorMessage = "Tên máy không được vượt quá 30 ký tự")]
        [Display(Name = "Tên máy")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại máy")]
        [Display(Name = "Loại máy")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại máy")]
        public int CatID { get; set; }

        [ForeignKey("CatID")]
        public virtual ComputerCategory? Category { get; set; }

        public virtual ICollection<UsageSession> UsageSessions { get; set; }
    }
}
