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
        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = username;
            ViewBag.Balance = user.Balance;
            ViewBag.ActiveSession = _context.UsageSessions
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Category)
                .FirstOrDefault(s => s.UserID == user.ID && s.EndTime == null);

            var computers = _context.Computers
                .Include(c => c.Category)
                .OrderBy(c => c.Name)
                .ToList();

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
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Kiểm tra xem người dùng có phiên đang hoạt động không
            var activeSession = await _context.UsageSessions
                .FirstOrDefaultAsync(s => s.UserID == user.ID && s.EndTime == null);
            
            if (activeSession != null)
            {
                TempData["Error"] = "Bạn đang có một phiên sử dụng máy khác đang hoạt động";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra máy tính có tồn tại và sẵn sàng không
            var computer = await _context.Computers
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.ID == computerId);

            if (computer == null)
            {
                TempData["Error"] = "Không tìm thấy máy tính";
                return RedirectToAction("Index", "Home");
            }

            if (computer.Status != ComputerStatus.Available)
            {
                TempData["Error"] = "Máy tính này không sẵn sàng để sử dụng";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra số dư
            if (user.Balance < computer.Category.PricePerHour)
            {
                TempData["Error"] = "Số dư của bạn không đủ để sử dụng máy này";
                return RedirectToAction("Index", "Home");
            }

            // Tạo phiên sử dụng mới
            var session = new UsageSession
            {
                UserID = user.ID,
                ComputerID = computer.ID,
                StartTime = DateTime.Now,
                TotalCost = 0
            };

            // Cập nhật trạng thái máy tính
            computer.Status = ComputerStatus.InUse;

            try
            {
                _context.UsageSessions.Add(session);
                _context.Computers.Update(computer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Bắt đầu sử dụng máy thành công";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiên sử dụng mới");
                TempData["Error"] = "Có lỗi xảy ra, vui lòng thử lại";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: User/EndSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndSession(int sessionId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login");

            var session = await _context.UsageSessions
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(s => s.ID == sessionId && s.UserID == user.ID && s.EndTime == null);

            if (session == null)
                return RedirectToAction("Index");

            session.EndTime = DateTime.Now;
            var duration = session.EndTime.Value - session.StartTime;
            var totalSeconds = (decimal)duration.TotalSeconds;
            var hours = Math.Floor(totalSeconds / 3600);
            var minutes = Math.Floor((totalSeconds % 3600) / 60);
            var seconds = totalSeconds % 60;
            
            // Tính chi phí theo giây
            var pricePerSecond = session.Computer.Category.PricePerHour / 3600m;
            session.TotalCost = totalSeconds * pricePerSecond;

            if (user.Balance < session.TotalCost)
            {
                TempData["Error"] = "Số dư không đủ để thanh toán phiên sử dụng";
                return RedirectToAction("Index");
            }

            // Cập nhật số dư người dùng
            user.Balance -= session.TotalCost.Value;
            _context.Users.Update(user);

            // Cập nhật trạng thái máy
            session.Computer.Status = ComputerStatus.Available;
            _context.Computers.Update(session.Computer);

            // Cập nhật doanh thu ngày
            var today = DateTime.Today;
            var dailyRevenue = await _context.DailyRevenues
                .FirstOrDefaultAsync(d => d.ReportDate.Date == today);

            if (dailyRevenue == null)
            {
                dailyRevenue = new DailyRevenue
                {
                    ReportDate = today,
                    TotalUsageRevenue = session.TotalCost.Value,
                    TotalServiceRevenue = 0,
                    TotalRecharge = 0
                };
                _context.DailyRevenues.Add(dailyRevenue);
                await _context.SaveChangesAsync();
            }
            else
            {
                dailyRevenue.TotalUsageRevenue += session.TotalCost.Value;
                _context.DailyRevenues.Update(dailyRevenue);
            }

            session.DailyRevenueID = dailyRevenue.ID;
            _context.UsageSessions.Update(session);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã kết thúc phiên sử dụng. Thời gian: {hours:00}:{minutes:00}:{seconds:00}. Chi phí: {session.TotalCost.Value:N0} VNĐ. Số dư còn lại: {user.Balance:N0} VNĐ";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi kết thúc phiên: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Recharge()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Recharge(decimal amount)
        {
            if (amount < 1000)
            {
                TempData["Error"] = "Số tiền nạp tối thiểu là 1,000 VNĐ";
                return RedirectToAction("Recharge");
            }

            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var request = new RechargeRequest
            {
                UserID = user.ID,
                Amount = amount,
                RequestTime = DateTime.Now,
                Status = RechargeStatus.Pending
            };

            _context.RechargeRequests.Add(request);

            // Tạo thông báo cho admin
            var notification = new Notification
            {
                UserID = user.ID,
                User = user,
                Title = "Yêu cầu nạp tiền",
                Content = $"Yêu cầu nạp {amount:N0} VNĐ của bạn đã được gửi",
                CreatedTime = DateTime.Now,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Yêu cầu nạp tiền đã được gửi, vui lòng chờ admin xử lý";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi gửi yêu cầu: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }
    }
} 