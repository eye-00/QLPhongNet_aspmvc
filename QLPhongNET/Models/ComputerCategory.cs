using System.ComponentModel.DataAnnotations;

namespace QLPhongNET.Models
{
    public class ComputerCategory
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal PricePerHour { get; set; }

        public ICollection<Computer> Computers { get; set; }
    }

}
