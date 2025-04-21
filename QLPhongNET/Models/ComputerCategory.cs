using System;
using System.ComponentModel.DataAnnotations;

namespace QLPhongNET.Models
{
    public class ComputerCategory
    {
        public ComputerCategory()
        {
            Name = string.Empty;
            PricePerHour = 0;
            Computers = new HashSet<Computer>();
        }

        public int ID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên loại máy")]
        [StringLength(50, ErrorMessage = "Tên loại máy không được vượt quá 50 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá giờ")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá giờ phải lớn hơn 0")]
        public decimal PricePerHour { get; set; }

        public virtual ICollection<Computer> Computers { get; set; }
    }
}
