using Microsoft.EntityFrameworkCore;
using QLPhongNET.Models;

public class QLPhongNetContext : DbContext
{
    public QLPhongNetContext(DbContextOptions<QLPhongNetContext> options)
    : base(options)
    {
    }
    public DbSet<ComputerCategory> ComputerCategories { get; set; }
    public DbSet<Computer> Computers { get; set; }
    public DbSet<User> Users { get; set; }
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
        // ComputerCategory
        modelBuilder.Entity<ComputerCategory>(entity =>
        {
            entity.ToTable("ComputerCategories");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PricePerHour).IsRequired().HasColumnType("decimal(15,2)");
        });

        // Computer
        modelBuilder.Entity<Computer>(entity =>
        {
            entity.ToTable("Computers");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.Status).HasDefaultValue("Available");
            entity.Property(e => e.CategoryID).HasColumnName("CatID");

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Computers)
                  .HasForeignKey(e => e.CategoryID);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.Username).HasMaxLength(30).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Password).HasMaxLength(30).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Balance).HasColumnType("decimal(15,2)").HasDefaultValue(0);
            entity.Property(e => e.Role).HasDefaultValue("User");
        });

        // UsageSession
        modelBuilder.Entity<UsageSession>(entity =>
        {
            entity.ToTable("UsageSession");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID");

            entity.Property(e => e.UserID).HasColumnName("UserID");
            entity.Property(e => e.ComputerID).HasColumnName("ComputerID");
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.TotalCost).HasColumnType("decimal(15,2)");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UsageSessions)
                  .HasForeignKey(e => e.UserID);

            entity.HasOne(e => e.Computer)
                  .WithMany(c => c.UsageSessions)
                  .HasForeignKey(e => e.ComputerID);
        });

        // Service
        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Services");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(15,2)").IsRequired();
        });

        // ServiceUsage
        modelBuilder.Entity<ServiceUsage>(entity =>
        {
            entity.ToTable("ServiceUsage");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID");

            entity.Property(e => e.UserID).HasColumnName("UserID");
            entity.Property(e => e.ServiceID).HasColumnName("ServiceID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UsageTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(15,2)");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.ServiceUsages)
                  .HasForeignKey(e => e.UserID);

            entity.HasOne(e => e.Service)
                  .WithMany(s => s.ServiceUsages)
                  .HasForeignKey(e => e.ServiceID);
        });

        // DailyRevenue
        modelBuilder.Entity<DailyRevenue>(entity =>
        {
            entity.ToTable("DailyRevenue");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.ReportDate).HasColumnType("date");
            entity.Property(e => e.TotalUsageRevenue).HasColumnType("decimal(15,2)");
            entity.Property(e => e.TotalRecharge).HasColumnType("decimal(15,2)");
            entity.Property(e => e.TotalServiceRevenue).HasColumnType("decimal(15,2)");
        });
    }
}
