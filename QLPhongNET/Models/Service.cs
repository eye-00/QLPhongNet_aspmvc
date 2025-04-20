using System.ComponentModel.DataAnnotations;

namespace QLPhongNET.Models
{
    public class Service
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public ICollection<ServiceUsage> ServiceUsages { get; set; }
    }
}
