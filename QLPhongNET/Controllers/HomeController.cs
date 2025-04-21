using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QLPhongNET.Models;
using QLPhongNET.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QLPhongNET.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QLPhongNetContext _context;

        public HomeController(ILogger<HomeController> logger, QLPhongNetContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Trang chính
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "User");
            }

            // Lấy phiên đang hoạt động của người dùng
            var activeSession = await _context.UsageSessions
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserID == user.ID && s.EndTime == null);

            var computers = await _context.Computers
                .Include(c => c.Category)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Lưu thông tin người dùng vào ViewBag
            ViewBag.Username = user.Username;
            ViewBag.Balance = user.Balance;
            ViewBag.Role = user.Role.ToString();
            ViewBag.ActiveSession = activeSession;
            ViewBag.UserID = user.ID;

            return View(computers);
        }

        // Trang tổng quan (Admin)
        public IActionResult Dashboard()
        {
            var totalUsers = _context.Users.Count();
            var totalComputers = _context.Computers.Count();
            var totalServices = _context.Services.Count();
            var todayRevenue = _context.DailyRevenues
                                       .Where(d => d.ReportDate == DateTime.Today)
                                       .FirstOrDefault();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalComputers = totalComputers;
            ViewBag.TotalServices = totalServices;
            ViewBag.TodayRevenue = todayRevenue;

            return View();
        }

        // Trang hồ sơ người dùng đang đăng nhập
        public IActionResult Profile()
        {
            var username = HttpContext.User.Identity.Name;

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // Trang giới thiệu
        public IActionResult About()
        {
            ViewData["Message"] = "Hệ thống quản lý quán net ASP.NET MVC.";
            return View();
        }

        // Trang liên hệ
        public IActionResult Contact()
        {
            ViewData["Message"] = "Liên hệ quản trị viên để biết thêm thông tin.";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
