using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;
using System.Diagnostics;

namespace QLPhongNET.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly QLPhongNetContext _context;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(QLPhongNetContext context, ILogger<StatisticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsAdmin()
        {
            var roleStr = HttpContext.Session.GetString("Role");
            return roleStr == UserRole.Admin.ToString();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsAdmin())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            base.OnActionExecuting(context);
        }

        // GET: Statistics
        public IActionResult Index()
        {
            return View();
        }

        // GET: Statistics/DailyDetails
        public async Task<IActionResult> DailyDetails(DateTime? date = null)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.Today;
                var now = DateTime.Now;

                // Thống kê theo máy
                var computerStats = await _context.UsageSessions
                    .Include(s => s.Computer)
                        .ThenInclude(c => c.Category)
                    .Where(s => s.StartTime.Date == targetDate && s.Computer != null && s.Computer.Category != null)
                    .ToListAsync();

                var computerStatsGrouped = computerStats
                    .GroupBy(s => new { s.ComputerID, s.Computer.Name, CategoryName = s.Computer.Category.Name })
                    .Select(g => new
                    {
                        ComputerID = g.Key.ComputerID,
                        ComputerName = g.Key.Name,
                        CategoryName = g.Key.CategoryName,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => (s.EndTime.HasValue ? s.EndTime.Value : now).Subtract(s.StartTime).TotalHours),
                        TotalRevenue = g.Sum(s => s.TotalCost)
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .ToList();

                // Thống kê theo người dùng
                var userStats = await _context.UsageSessions
                    .Include(s => s.User)
                    .Where(s => s.StartTime.Date == targetDate && s.User != null)
                    .ToListAsync();

                var userStatsGrouped = userStats
                    .GroupBy(s => new { s.UserID, s.User.Username, s.User.FullName })
                    .Select(g => new
                    {
                        UserID = g.Key.UserID,
                        Username = g.Key.Username,
                        FullName = g.Key.FullName,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => (s.EndTime.HasValue ? s.EndTime.Value : now).Subtract(s.StartTime).TotalHours),
                        TotalSpent = g.Sum(s => s.TotalCost)
                    })
                    .OrderByDescending(s => s.TotalSpent)
                    .ToList();

                // Thống kê dịch vụ
                var serviceStats = await _context.ServiceUsages
                    .Include(su => su.Service)
                    .Where(su => su.UsageTime.Date == targetDate && su.Service != null)
                    .ToListAsync();

                var serviceStatsGrouped = serviceStats
                    .GroupBy(su => new { su.ServiceID, su.Service.Name })
                    .Select(g => new
                    {
                        ServiceID = g.Key.ServiceID,
                        ServiceName = g.Key.Name,
                        TotalQuantity = g.Sum(su => su.Quantity),
                        TotalRevenue = g.Sum(su => su.TotalPrice)
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .ToList();

                // Thống kê nạp tiền
                var rechargeStats = await _context.DailyRevenues
                    .Where(r => r.ReportDate == targetDate)
                    .Select(r => new
                    {
                        TotalRecharge = r.TotalRecharge,
                        TotalUsageRevenue = r.TotalUsageRevenue,
                        TotalServiceRevenue = r.TotalServiceRevenue
                    })
                    .FirstOrDefaultAsync() ?? new
                    {
                        TotalRecharge = 0m,
                        TotalUsageRevenue = 0m,
                        TotalServiceRevenue = 0m
                    };

                ViewBag.ComputerStats = computerStatsGrouped;
                ViewBag.UserStats = userStatsGrouped;
                ViewBag.ServiceStats = serviceStatsGrouped;
                ViewBag.RechargeStats = rechargeStats;
                ViewBag.Date = targetDate;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê chi tiết doanh thu trong ngày");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View();
            }
        }

        // GET: Statistics/ByComputer
        public async Task<IActionResult> ByComputer(DateTime? fromDate = null, DateTime? toDate = null, string groupBy = "day")
        {
            try
            {
                var query = _context.UsageSessions
                    .Include(s => s.Computer)
                    .Include(s => s.Computer.Category)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(s => s.StartTime.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(s => s.StartTime.Date <= toDate.Value.Date);
                }

                // Lấy dữ liệu từ database trước
                var sessions = await query.ToListAsync();
                
                // Tính toán thống kê ở phía client
                var computerStats = sessions
                    .GroupBy(s => new { 
                        ComputerId = s.Computer.ID, 
                        ComputerName = s.Computer.Name, 
                        CategoryName = s.Computer.Category.Name 
                    })
                    .Select(g => new
                    {
                        ComputerId = g.Key.ComputerId,
                        ComputerName = g.Key.ComputerName,
                        CategoryName = g.Key.CategoryName,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => s.EndTime.HasValue 
                            ? (s.EndTime.Value - s.StartTime).TotalHours 
                            : (DateTime.Now - s.StartTime).TotalHours),
                        TotalRevenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .ToList();

                // Tính tổng số máy
                var totalComputers = await _context.Computers.CountAsync();
                
                // Tính tổng số phiên
                var totalSessions = computerStats.Sum(s => s.TotalSessions);
                
                // Tính tổng giờ sử dụng
                var totalHours = computerStats.Sum(s => s.TotalHours);
                
                // Tính tổng doanh thu
                var totalRevenue = computerStats.Sum(s => s.TotalRevenue);

                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.GroupBy = groupBy;
                ViewBag.TotalComputers = totalComputers;
                ViewBag.TotalSessions = totalSessions;
                ViewBag.TotalHours = totalHours;
                ViewBag.TotalRevenue = totalRevenue;

                return View(computerStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê theo máy");
                TempData["Error"] = "Có lỗi xảy ra khi lấy thống kê";
                return View(new List<dynamic>());
            }
        }

        // GET: Statistics/ByUser
        public async Task<IActionResult> ByUser(DateTime? fromDate = null, DateTime? toDate = null, string groupBy = "day")
        {
            try
            {
                var query = _context.UsageSessions
                    .Include(u => u.User)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(u => u.StartTime.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(u => u.StartTime.Date <= toDate.Value.Date);
                }

                // Lấy dữ liệu từ database trước
                var sessions = await query.ToListAsync();
                
                // Tính toán thống kê ở phía client
                var userStats = sessions
                    .GroupBy(u => new { u.UserID, u.User.Username, u.User.FullName })
                    .Select(g => new UserPeriodStatistics
                    {
                        UserID = g.Key.UserID,
                        Username = g.Key.Username,
                        FullName = g.Key.FullName,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => (s.EndTime.HasValue ? s.EndTime.Value : DateTime.Now).Subtract(s.StartTime).TotalHours),
                        UsageSpent = g.Sum(s => s.TotalCost ?? 0),
                        ServiceSpent = 0,
                        TotalSpent = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderByDescending(s => s.TotalSpent)
                    .ToList();

                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.GroupBy = groupBy;

                return View(userStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê theo người dùng");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View(new List<UserPeriodStatistics>());
            }
        }

        // GET: Statistics/ByTime
        public async Task<IActionResult> ByTime(string groupBy = "day", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                if (!fromDate.HasValue)
                {
                    fromDate = DateTime.Today.AddDays(-30);
                }
                if (!toDate.HasValue)
                {
                    toDate = DateTime.Today;
                }

                var now = DateTime.Now;

                // Lấy dữ liệu từ database trước
                var sessions = await _context.UsageSessions
                    .Where(s => s.StartTime.Date >= fromDate.Value.Date && 
                               s.StartTime.Date <= toDate.Value.Date)
                    .ToListAsync();

                // Tính toán thống kê theo nhóm thời gian
                var stats = sessions
                    .GroupBy(s => new
                    {
                        Year = s.StartTime.Year,
                        Month = s.StartTime.Month,
                        Day = s.StartTime.Day,
                        Week = (s.StartTime.Day - 1) / 7 + 1
                    })
                    .Select(g => new
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                        Week = g.Key.Week,
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => 
                        {
                            var endTime = s.EndTime ?? now;
                            return (endTime - s.StartTime).TotalHours;
                        }),
                        TotalRevenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                // Nhóm dữ liệu theo thời gian
                IEnumerable<dynamic> groupedStats = groupBy switch
                {
                    "week" => stats
                        .GroupBy(s => new { s.Year, s.Month, s.Week })
                        .Select(g => new
                        {
                            Date = g.First().Date,
                            Week = g.Key.Week,
                            TotalSessions = g.Sum(s => s.TotalSessions),
                            TotalHours = g.Sum(s => s.TotalHours),
                            TotalRevenue = g.Sum(s => s.TotalRevenue)
                        })
                        .OrderBy(s => s.Date)
                        .ToList(),
                    "month" => stats
                        .GroupBy(s => new { s.Year, s.Month })
                        .Select(g => new
                        {
                            Date = g.First().Date,
                            TotalSessions = g.Sum(s => s.TotalSessions),
                            TotalHours = g.Sum(s => s.TotalHours),
                            TotalRevenue = g.Sum(s => s.TotalRevenue)
                        })
                        .OrderBy(s => s.Date)
                        .ToList(),
                    "year" => stats
                        .GroupBy(s => s.Year)
                        .Select(g => new
                        {
                            Date = g.First().Date,
                            TotalSessions = g.Sum(s => s.TotalSessions),
                            TotalHours = g.Sum(s => s.TotalHours),
                            TotalRevenue = g.Sum(s => s.TotalRevenue)
                        })
                        .OrderBy(s => s.Date)
                        .ToList(),
                    _ => stats
                };

                ViewBag.GroupBy = groupBy;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                return View(groupedStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê theo thời gian");
                TempData["Error"] = "Có lỗi xảy ra khi lấy thống kê";
                return View(new List<dynamic>());
            }
        }

        // GET: Statistics/Revenue
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate, string groupBy = "day")
        {
            try
            {
                var query = _context.DailyRevenues.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.ReportDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(r => r.ReportDate <= endDate.Value);

                var statistics = await query
                    .OrderBy(r => r.ReportDate)
                    .Select(r => new
                    {
                        Date = r.ReportDate,
                        Year = r.ReportDate.Year,
                        Month = r.ReportDate.Month,
                        // Tính ngày đầu tuần (thứ 2)
                        WeekStart = r.ReportDate.AddDays(-((int)r.ReportDate.DayOfWeek - 1 + 7) % 7),
                        // Tính ngày cuối tuần (chủ nhật)
                        WeekEnd = r.ReportDate.AddDays(-((int)r.ReportDate.DayOfWeek - 1 + 7) % 7).AddDays(6),
                        MonthStart = new DateTime(r.ReportDate.Year, r.ReportDate.Month, 1),
                        MonthEnd = new DateTime(r.ReportDate.Year, r.ReportDate.Month, DateTime.DaysInMonth(r.ReportDate.Year, r.ReportDate.Month)),
                        YearStart = new DateTime(r.ReportDate.Year, 1, 1),
                        YearEnd = new DateTime(r.ReportDate.Year, 12, 31),
                        RechargeRevenue = r.TotalRecharge,
                        UsageRevenue = r.TotalUsageRevenue,
                        ServiceRevenue = r.TotalServiceRevenue,
                        TotalRevenue = r.TotalUsageRevenue + r.TotalServiceRevenue
                    })
                    .ToListAsync();

                // Nhóm dữ liệu theo thời gian
                IEnumerable<dynamic> groupedStatistics = groupBy switch
                {
                    "week" => statistics
                        .GroupBy(s => new { s.Year, s.WeekStart })
                        .Select(g => new
                        {
                            Date = g.First().Date,
                            WeekStart = g.Key.WeekStart,
                            WeekEnd = g.First().WeekEnd,
                            RechargeRevenue = g.Sum(s => s.RechargeRevenue),
                            UsageRevenue = g.Sum(s => s.UsageRevenue),
                            ServiceRevenue = g.Sum(s => s.ServiceRevenue),
                            TotalRevenue = g.Sum(s => s.TotalRevenue)
                        })
                        .OrderBy(s => s.WeekStart)
                        .ToList(),
                    "month" => statistics
                        .GroupBy(s => new { s.Year, s.Month })
                        .Select(g => new
                        {
                            Date = g.First().Date,
                            StartDate = g.First().MonthStart,
                            EndDate = g.First().MonthEnd,
                            RechargeRevenue = g.Sum(s => s.RechargeRevenue),
                            UsageRevenue = g.Sum(s => s.UsageRevenue),
                            ServiceRevenue = g.Sum(s => s.ServiceRevenue),
                            TotalRevenue = g.Sum(s => s.TotalRevenue)
                        })
                        .OrderBy(s => s.StartDate)
                        .ToList(),
                    "year" => statistics
                        .GroupBy(s => s.Year)
                        .Select(g => new
                        {
                            Date = g.First().Date,
                            StartDate = g.First().YearStart,
                            EndDate = g.First().YearEnd,
                            RechargeRevenue = g.Sum(s => s.RechargeRevenue),
                            UsageRevenue = g.Sum(s => s.UsageRevenue),
                            ServiceRevenue = g.Sum(s => s.ServiceRevenue),
                            TotalRevenue = g.Sum(s => s.TotalRevenue)
                        })
                        .OrderBy(s => s.StartDate)
                        .ToList(),
                    _ => statistics.Select(s => new
                    {
                        s.Date,
                        StartDate = s.Date,
                        EndDate = s.Date,
                        s.RechargeRevenue,
                        s.UsageRevenue,
                        s.ServiceRevenue,
                        s.TotalRevenue
                    }).ToList()
                };

                var summary = new
                {
                    TotalRechargeRevenue = groupedStatistics.Sum(s => (decimal)s.RechargeRevenue),
                    TotalUsageRevenue = groupedStatistics.Sum(s => (decimal)s.UsageRevenue),
                    TotalServiceRevenue = groupedStatistics.Sum(s => (decimal)s.ServiceRevenue),
                    GrandTotal = groupedStatistics.Sum(s => (decimal)s.TotalRevenue)
                };

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.GroupBy = groupBy;
                ViewBag.Summary = summary;
                return View(groupedStatistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê doanh thu");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View(new List<object>());
            }
        }

        // GET: Statistics/PeriodDetails
        public async Task<IActionResult> PeriodDetails(int? computerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                if (!fromDate.HasValue)
                {
                    fromDate = DateTime.Today.AddDays(-30);
                }
                if (!toDate.HasValue)
                {
                    toDate = DateTime.Today;
                }

                // Lấy thông tin máy
                var computer = await _context.Computers
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.ID == computerId);

                if (computer == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin máy";
                    return RedirectToAction(nameof(ByComputer));
                }

                var now = DateTime.Now;

                // Lấy thống kê theo ngày
                var sessions = await _context.UsageSessions
                    .Where(s => s.ComputerID == computerId &&
                               s.StartTime.Date >= fromDate.Value.Date &&
                               s.StartTime.Date <= toDate.Value.Date)
                    .ToListAsync();

                var dailyStats = sessions
                    .GroupBy(s => s.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Sessions = g.Count(),
                        Hours = g.Sum(s => 
                        {
                            var endTime = s.EndTime ?? now;
                            return (endTime - s.StartTime).TotalHours;
                        }),
                        Revenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderByDescending(s => s.Date)
                    .ToList();

                // Tính tổng các chỉ số
                var totalSessions = dailyStats.Sum(s => s.Sessions);
                var totalHours = dailyStats.Sum(s => s.Hours);
                var totalRevenue = dailyStats.Sum(s => s.Revenue);

                // Gán dữ liệu cho view
                ViewBag.ComputerId = computer.ID;
                ViewBag.ComputerName = computer.Name;
                ViewBag.CategoryName = computer.Category.Name;
                ViewBag.PricePerHour = computer.Category.PricePerHour;
                ViewBag.Status = computer.Status;
                ViewBag.TotalSessions = totalSessions;
                ViewBag.TotalHours = totalHours;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.DailyStats = dailyStats;
                ViewBag.FromDate = fromDate.Value;
                ViewBag.ToDate = toDate.Value;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê chi tiết theo khoảng thời gian");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> ComputerDetails(int computerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var computer = await _context.Computers
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.ID == computerId);

                if (computer == null)
                {
                    TempData["Error"] = "Không tìm thấy máy tính";
                    return RedirectToAction(nameof(ByComputer));
                }

                var query = _context.UsageSessions
                    .Include(s => s.User)
                    .Where(s => s.ComputerID == computerId)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(s => s.StartTime.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(s => s.StartTime.Date <= toDate.Value.Date);
                }

                // Lấy dữ liệu từ database trước
                var sessions = await query.ToListAsync();
                
                // Tính toán thống kê theo ngày
                var dailyStats = sessions
                    .GroupBy(s => s.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => s.EndTime.HasValue 
                            ? (s.EndTime.Value - s.StartTime).TotalHours 
                            : (DateTime.Now - s.StartTime).TotalHours),
                        TotalRevenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderByDescending(s => s.Date)
                    .ToList();

                // Tính tổng số phiên
                var totalSessions = dailyStats.Sum(s => s.TotalSessions);
                
                // Tính tổng giờ sử dụng
                var totalHours = dailyStats.Sum(s => s.TotalHours);
                
                // Tính tổng doanh thu
                var totalRevenue = dailyStats.Sum(s => s.TotalRevenue);

                ViewBag.Computer = computer;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.TotalSessions = totalSessions;
                ViewBag.TotalHours = totalHours;
                ViewBag.TotalRevenue = totalRevenue;

                return View(dailyStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết thống kê theo máy");
                TempData["Error"] = "Có lỗi xảy ra khi lấy chi tiết thống kê";
                return RedirectToAction(nameof(ByComputer));
            }
        }
    }
} 