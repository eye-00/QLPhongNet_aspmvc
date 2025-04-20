using Microsoft.AspNetCore.Mvc;
using QLPhongNET.Models;
using System.Linq;

namespace QLPhongNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly QLPhongNetContext _context;

        public HomeController(QLPhongNetContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult Index()
        {
            return View();
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
    }
}
