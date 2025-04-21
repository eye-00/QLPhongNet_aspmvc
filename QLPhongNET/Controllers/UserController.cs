using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;
using System.Diagnostics;

namespace QLPhongNET.Controllers
{
    public class UserController : Controller
    {
        private readonly QLPhongNetContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(QLPhongNetContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: User/Index
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .Include(u => u.UsageSessions.Where(s => s.EndTime == null))
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var computers = await _context.Computers
                .Include(c => c.Category)
                .ToListAsync();

            var services = await _context.Services.ToListAsync();

            ViewBag.CurrentUser = user;
            ViewBag.CurrentSession = user.UsageSessions.FirstOrDefault();
            ViewBag.Services = services;

            return View(computers);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserID", user.ID.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName ?? string.Empty);
                HttpContext.Session.SetString("Role", user.Role.ToString());

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(user);
                }

                user.Role = UserRole.User;
                user.Balance = 0;
                _context.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(user);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .Include(u => u.UsageSessions)
                .Include(u => u.ServiceUsages)
                .FirstOrDefaultAsync(u => u.ID.ToString() == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.ID == id);
        }

        // POST: User/StartSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSession(int computerId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var computer = await _context.Computers
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.ID == computerId);

            if (computer == null || computer.Status != ComputerStatus.Available)
            {
                TempData["Error"] = "Máy tính không khả dụng";
                return RedirectToAction(nameof(Index));
            }

            var currentSession = await _context.UsageSessions
                .FirstOrDefaultAsync(s => s.UserID == user.ID && s.EndTime == null);

            if (currentSession != null)
            {
                TempData["Error"] = "Bạn đang có phiên sử dụng khác";
                return RedirectToAction(nameof(Index));
            }

            var session = new UsageSession
            {
                UserID = user.ID,
                ComputerID = computerId,
                StartTime = DateTime.Now
            };

            computer.Status = ComputerStatus.InUse;
            _context.UsageSessions.Add(session);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: User/EndSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndSession(int sessionId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var session = await _context.UsageSessions
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(s => s.ID == sessionId && s.UserID == user.ID);

            if (session == null)
            {
                TempData["Error"] = "Không tìm thấy phiên sử dụng";
                return RedirectToAction(nameof(Index));
            }

            session.EndTime = DateTime.Now;
            var duration = (session.EndTime.Value - session.StartTime).TotalHours;
            session.TotalCost = (decimal)(duration * (double)session.Computer.Category.PricePerHour);

            // Cập nhật số dư người dùng
            if (user.Balance < session.TotalCost)
            {
                TempData["Error"] = "Số dư không đủ để thanh toán phiên sử dụng";
                return RedirectToAction(nameof(Index));
            }

            user.Balance -= session.TotalCost.Value;
            session.Computer.Status = ComputerStatus.Available;

            // Cập nhật doanh thu ngày
            var today = DateTime.Today;
            var dailyRevenue = await _context.DailyRevenues
                .FirstOrDefaultAsync(d => d.ReportDate.Date == today);

            if (dailyRevenue == null)
            {
                dailyRevenue = new DailyRevenue
                {
                    ReportDate = today,
                    TotalUsageRevenue = 0,
                    TotalRecharge = 0,
                    TotalServiceRevenue = 0
                };
                _context.DailyRevenues.Add(dailyRevenue);
                await _context.SaveChangesAsync(); // Lưu để lấy ID
            }

            dailyRevenue.TotalUsageRevenue += session.TotalCost.Value;
            session.DailyRevenueID = dailyRevenue.ID;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã kết thúc phiên sử dụng. Chi phí: {session.TotalCost.Value:N0} VNĐ";

            return RedirectToAction(nameof(Index));
        }
    }
} 