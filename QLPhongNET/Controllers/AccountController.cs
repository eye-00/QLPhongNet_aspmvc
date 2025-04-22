using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;
using System.Diagnostics;

namespace QLPhongNET.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLPhongNetContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(QLPhongNetContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Kết thúc phiên đăng nhập cũ nếu có
                var oldSession = await _context.LoginSessions
                    .FirstOrDefaultAsync(s => s.UserID == user.ID && s.IsActive);
                if (oldSession != null)
                {
                    oldSession.IsActive = false;
                    oldSession.LogoutTime = DateTime.Now;
                }

                // Tạo phiên đăng nhập mới
                var newSession = new LoginSession
                {
                    UserID = user.ID,
                    LoginTime = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    IsActive = true
                };
                _context.LoginSessions.Add(newSession);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role.ToString());
                HttpContext.Session.SetInt32("UserID", user.ID);
                HttpContext.Session.SetInt32("LoginSessionID", newSession.ID);

                if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var loginSessionId = HttpContext.Session.GetInt32("LoginSessionID");
            if (loginSessionId.HasValue)
            {
                var session = await _context.LoginSessions.FindAsync(loginSessionId.Value);
                if (session != null)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
} 