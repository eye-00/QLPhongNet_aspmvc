using Microsoft.EntityFrameworkCore;
using QLPhongNET.Models;

namespace QLPhongNET.Data
{
    public class QLPhongNetContext : DbContext
    {
        public QLPhongNetContext(DbContextOptions<QLPhongNetContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<ComputerCategory> ComputerCategories { get; set; }
        public DbSet<UsageSession> UsageSessions { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceUsage> ServiceUsages { get; set; }
        public DbSet<DailyRevenue> DailyRevenues { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseMySql("server=vpnphake.ddns.net;database=QLPhongNet_ASPMVC;user=suisei0227;password=Suisei@0227",
        //        new MySqlServerVersion(new Version(8, 0, 21))); // Chỉnh sửa thông tin kết nối
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Computer
            modelBuilder.Entity<Computer>(entity =>
            {
                entity.ToTable("Computers");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CatID).IsRequired();
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Computers)
                    .HasForeignKey(e => e.CatID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ComputerCategory
            modelBuilder.Entity<ComputerCategory>(entity =>
            {
                entity.ToTable("ComputerCategories");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PricePerHour).IsRequired();
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(30);
                entity.Property(e => e.FullName).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.Balance).HasDefaultValue(0);
                entity.Property(e => e.Role).IsRequired();
            });

            // UsageSession
            modelBuilder.Entity<UsageSession>(entity =>
            {
                entity.ToTable("UsageSession");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.TotalCost).HasPrecision(15, 2);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UsageSessions)
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Computer)
                    .WithMany()
                    .HasForeignKey(e => e.ComputerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DailyRevenue)
                    .WithMany()
                    .HasForeignKey(e => e.DailyRevenueID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Service
            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Services");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).IsRequired().HasPrecision(15, 2);
            });

            // ServiceUsage
            modelBuilder.Entity<ServiceUsage>(entity =>
            {
                entity.ToTable("ServiceUsage");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                entity.Property(e => e.UsageTime).IsRequired();
                entity.Property(e => e.TotalPrice).HasPrecision(15, 2);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.ServiceUsages)
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DailyRevenue)
                    .WithMany()
                    .HasForeignKey(e => e.DailyRevenueID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DailyRevenue
            modelBuilder.Entity<DailyRevenue>(entity =>
            {
                entity.ToTable("DailyRevenue");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ReportDate).IsRequired();
                entity.Property(e => e.TotalUsageRevenue).HasPrecision(15, 2);
                entity.Property(e => e.TotalRecharge).HasPrecision(15, 2);
                entity.Property(e => e.TotalServiceRevenue).HasPrecision(15, 2);
            });

            // Cấu hình cho UsageSession
            modelBuilder.Entity<UsageSession>()
                .HasOne(s => s.User)
                .WithMany(u => u.UsageSessions)
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UsageSession>()
                .HasOne(s => s.Computer)
                .WithMany(c => c.UsageSessions)
                .HasForeignKey(s => s.ComputerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình cho ServiceUsage
            modelBuilder.Entity<ServiceUsage>()
                .HasOne(su => su.User)
                .WithMany(u => u.ServiceUsages)
                .HasForeignKey(su => su.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceUsage>()
                .HasOne(su => su.Service)
                .WithMany(s => s.ServiceUsages)
                .HasForeignKey(su => su.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình cho DailyRevenue
            modelBuilder.Entity<DailyRevenue>()
                .HasMany(d => d.UsageSessions)
                .WithOne(s => s.DailyRevenue)
                .HasForeignKey(s => s.DailyRevenueID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyRevenue>()
                .HasMany(d => d.ServiceUsages)
                .WithOne(su => su.DailyRevenue)
                .HasForeignKey(su => su.DailyRevenueID)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình chuyển đổi cho ComputerStatus
            modelBuilder.Entity<Computer>()
                .Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (ComputerStatus)Enum.Parse(typeof(ComputerStatus), v)
                );
        }
    }
}
