using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;
using System.Diagnostics;

namespace QLPhongNET.Controllers
{
    public class RevenueController : Controller
    {
        private readonly QLPhongNetContext _context;
        private readonly ILogger<RevenueController> _logger;

        public RevenueController(QLPhongNetContext context, ILogger<RevenueController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Revenue
        public async Task<IActionResult> Index()
        {
            // Lấy thống kê theo ngày
            var today = DateTime.Today;
            var dailyRevenue = await _context.DailyRevenues
                .Where(d => d.ReportDate == today)
                .FirstOrDefaultAsync();

            if (dailyRevenue == null)
            {
                // Nếu chưa có báo cáo cho ngày hôm nay, tạo mới
                dailyRevenue = new DailyRevenue
                {
                    ReportDate = today,
                    TotalUsageRevenue = 0,
                    TotalRecharge = 0,
                    TotalServiceRevenue = 0
                };
                _context.DailyRevenues.Add(dailyRevenue);
                await _context.SaveChangesAsync();
            }

            // Lấy thống kê theo tuần
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var weeklyRevenue = await _context.DailyRevenues
                .Where(d => d.ReportDate >= startOfWeek && d.ReportDate <= today)
                .GroupBy(d => d.ReportDate)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(d => d.TotalUsageRevenue + d.TotalRecharge + d.TotalServiceRevenue)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Lấy thống kê theo tháng
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var monthlyRevenue = await _context.DailyRevenues
                .Where(d => d.ReportDate >= startOfMonth && d.ReportDate <= today)
                .GroupBy(d => d.ReportDate)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(d => d.TotalUsageRevenue + d.TotalRecharge + d.TotalServiceRevenue)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.DailyRevenue = dailyRevenue;
            ViewBag.WeeklyRevenue = weeklyRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;

            return View();
        }

        // GET: Revenue/Details/5
        public async Task<IActionResult> Details(DateTime date)
        {
            var dailyRevenue = await _context.DailyRevenues
                .FirstOrDefaultAsync(d => d.ReportDate == date);

            if (dailyRevenue == null)
            {
                return NotFound();
            }

            // Lấy chi tiết các phiên sử dụng trong ngày
            var usageSessions = await _context.UsageSessions
                .Include(u => u.Computer)
                .Include(u => u.User)
                .Where(u => u.StartTime.Date == date)
                .OrderByDescending(u => u.StartTime)
                .ToListAsync();

            // Lấy chi tiết các dịch vụ được sử dụng trong ngày
            var serviceUsages = await _context.ServiceUsages
                .Include(s => s.Service)
                .Include(s => s.User)
                .Where(s => s.UsageTime.Date == date)
                .OrderByDescending(s => s.UsageTime)
                .ToListAsync();

            ViewBag.UsageSessions = usageSessions;
            ViewBag.ServiceUsages = serviceUsages;

            return View(dailyRevenue);
        }
    }
} 