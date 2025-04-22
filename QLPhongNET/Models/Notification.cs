using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongNET.Models
{
    public class Notification
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public required User User { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Content { get; set; }

        [Required]
        public DateTime CreatedTime { get; set; }

        public bool IsRead { get; set; }

        public string? Link { get; set; }
    }
} 