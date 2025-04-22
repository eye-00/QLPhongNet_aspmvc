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

        // GET: Statistics/ByComputer
        public async Task<IActionResult> ByComputer(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.UsageSessions
                    .Include(s => s.Computer)
                        .ThenInclude(c => c.Category)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(s => s.StartTime >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(s => s.StartTime <= endDate.Value);

                var statistics = await query
                    .GroupBy(s => new { s.ComputerID, s.Computer.Name, CategoryName = s.Computer.Category.Name })
                    .Select(g => new
                    {
                        ComputerID = g.Key.ComputerID,
                        ComputerName = g.Key.Name,
                        CategoryName = g.Key.CategoryName,
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => (double)((s.EndTime ?? DateTime.Now) - s.StartTime).TotalHours),
                        TotalRevenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .ToListAsync();

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê theo máy tính");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View(new List<object>());
            }
        }

        // GET: Statistics/ByUser
        public async Task<IActionResult> ByUser(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.UsageSessions
                    .Include(s => s.User)
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
                        TotalSpent = g.Sum(s => s.TotalCost ?? 0)
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
        public async Task<IActionResult> ByTime(string groupBy = "day")
        {
            try
            {
                var query = _context.UsageSessions.AsQueryable();
                var now = DateTime.Now;
                var startDate = groupBy switch
                {
                    "week" => now.AddDays(-7),
                    "month" => now.AddMonths(-1),
                    "year" => now.AddYears(-1),
                    _ => now.AddDays(-30) // Mặc định là 30 ngày
                };

                query = query.Where(s => s.StartTime >= startDate);

                var statistics = await query
                    .GroupBy(s => new
                    {
                        Year = s.StartTime.Year,
                        Month = s.StartTime.Month,
                        Day = s.StartTime.Day,
                        Hour = s.StartTime.Hour
                    })
                    .Select(g => new
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                        TotalSessions = g.Count(),
                        TotalHours = g.Sum(s => ((s.EndTime ?? DateTime.Now) - s.StartTime).TotalHours),
                        TotalRevenue = g.Sum(s => s.TotalCost ?? 0)
                    })
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                ViewBag.GroupBy = groupBy;
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê theo thời gian");
                TempData["Error"] = "Có lỗi xảy ra khi thống kê: " + ex.Message;
                return View(new List<object>());
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