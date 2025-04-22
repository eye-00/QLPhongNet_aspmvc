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
        public IActionResult ByComputer(DateTime? fromDate, DateTime? toDate)
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

                var stats = query
                    .GroupBy(s => new { 
                        ComputerId = s.Computer.ID, 
                        ComputerName = s.Computer.Name, 
                        CategoryName = s.Computer.Category.Name 
                    })
                    .Select(g => new
                    {
                        ComputerName = g.Key.ComputerName,
                        CategoryName = g.Key.CategoryName,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => (double)((s.EndTime ?? DateTime.Now) - s.StartTime).TotalHours),
                        TotalRevenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .ToList();

                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê theo máy");
                TempData["Error"] = "Có lỗi xảy ra khi lấy thống kê";
                return View(new List<dynamic>());
            }
        }

        // GET: Statistics/ByUser
        public async Task<IActionResult> ByUser(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.UsageSessions
                    .Include(s => s.User)
                    .Where(s => s.User != null)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(s => s.StartTime >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(s => s.StartTime <= endDate.Value);

                var statistics = await query
                    .GroupBy(s => new { s.UserID, s.User.Username, s.User.FullName })
                    .Select(g => new
                    {
                        UserID = g.Key.UserID,
                        Username = g.Key.Username,
                        FullName = g.Key.FullName,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => (double)((s.EndTime ?? DateTime.Now) - s.StartTime).TotalHours),
                        TotalSpent = g.Sum(s => (decimal?)s.TotalCost ?? 0m)
                    })
                    .OrderByDescending(s => s.TotalSpent)
                    .ToListAsync();

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê theo người dùng");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View(new List<object>());
            }
        }

        // GET: Statistics/ByTime
        public IActionResult ByTime(string groupBy = "day", DateTime? fromDate = null, DateTime? toDate = null)
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

                var query = _context.UsageSessions.AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(s => s.StartTime.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(s => s.StartTime.Date <= toDate.Value.Date);
                }

                var stats = query
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
                        TotalHours = g.Sum(s => (double)((s.EndTime ?? DateTime.Now) - s.StartTime).TotalHours),
                        TotalRevenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                ViewBag.GroupBy = groupBy;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê theo thời gian");
                TempData["Error"] = "Có lỗi xảy ra khi lấy thống kê";
                return View(new List<dynamic>());
            }
        }

        // GET: Statistics/Revenue
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
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
                        RechargeRevenue = r.TotalRecharge,
                        UsageRevenue = r.TotalUsageRevenue,
                        ServiceRevenue = r.TotalServiceRevenue,
                        TotalRevenue = r.TotalRecharge + r.TotalUsageRevenue + r.TotalServiceRevenue
                    })
                    .ToListAsync();

                var summary = new
                {
                    TotalRechargeRevenue = statistics.Sum(s => s.RechargeRevenue),
                    TotalUsageRevenue = statistics.Sum(s => s.UsageRevenue),
                    TotalServiceRevenue = statistics.Sum(s => s.ServiceRevenue),
                    GrandTotal = statistics.Sum(s => s.TotalRevenue)
                };

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.Summary = summary;
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê doanh thu");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View(new List<object>());
            }
        }
    }
} 