namespace QLPhongNET.Models
{
    public class UserPeriodStatistics
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public double TotalHours { get; set; }
        public decimal UsageSpent { get; set; }
        public decimal ServiceSpent { get; set; }
        public decimal TotalSpent { get; set; }
    }
} 