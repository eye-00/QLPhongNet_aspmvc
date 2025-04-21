using System.ComponentModel.DataAnnotations;

namespace QLPhongNET.Models
{
    public class Service
    {
        public Service()
        {
            Name = string.Empty;
            Price = 0;
            ServiceUsages = new HashSet<ServiceUsage>();
        }

        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        [StringLength(50, ErrorMessage = "Tên dịch vụ không được vượt quá 50 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải lớn hơn 0")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        public virtual ICollection<ServiceUsage> ServiceUsages { get; set; }
    }
}
