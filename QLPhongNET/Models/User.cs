namespace QLPhongNET.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public decimal Balance { get; set; }
        public string Role { get; set; }

        public ICollection<UsageSession> UsageSessions { get; set; }
        public ICollection<ServiceUsage> ServiceUsages { get; set; }
    }
}
