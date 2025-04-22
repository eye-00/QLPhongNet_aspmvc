using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;
using System.Diagnostics;

namespace QLPhongNET.Controllers
{
    public class AdminController : Controller
    {
        private readonly QLPhongNetContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(QLPhongNetContext context, ILogger<AdminController> logger)
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

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalComputers = await _context.Computers.CountAsync();
            ViewBag.ComputersInUse = await _context.Computers.CountAsync(c => c.Status == ComputerStatus.InUse);
            ViewBag.ComputersMaintenance = await _context.Computers.CountAsync(c => c.Status == ComputerStatus.Maintenance);
            return View();
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Admin/Users/Create
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Users));
            }
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> EditUser(int? id)
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

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = _context.Users.Find(model.ID);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                    return RedirectToAction("Users");
                }

                // Kiểm tra username đã tồn tại chưa (trừ user hiện tại)
                var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.ID != model.ID);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Cập nhật thông tin cơ bản
                user.Username = model.Username;
                user.FullName = model.FullName;
                user.Phone = model.Phone;
                user.Balance = model.Balance;
                user.Role = model.Role;

                // Chỉ cập nhật mật khẩu nếu người dùng nhập mật khẩu mới
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }

                _context.SaveChanges();
                TempData["Success"] = "Cập nhật người dùng thành công";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật người dùng";
                return View(model);
            }
        }

        // GET: Admin/Computers
        public async Task<IActionResult> Computers()
        {
            var computers = await _context.Computers
                .Include(c => c.Category)
                .ToListAsync();
            return View(computers);
        }

        // GET: Admin/Computers/Create
        public IActionResult CreateComputer()
        {
            var categories = _context.ComputerCategories.ToList();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "ID", "Name");
            return View();
        }

        // POST: Admin/Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComputer(Computer computer)
        {
            _logger.LogInformation("Bắt đầu thêm máy tính mới: {Name}", computer.Name);
            
            if (ModelState.IsValid)
            {
                try
                {
                    computer.Status = ComputerStatus.Available;
                    
                    _logger.LogInformation("Thêm máy tính vào context: {Name}, Status: {Status}, CatID: {CatID}", 
                        computer.Name, computer.Status, computer.CatID);
                    
                    _context.Add(computer);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Thêm máy tính thành công với ID: {ID}", computer.ID);
                    
                    TempData["Success"] = "Thêm máy tính mới thành công!";
                    return RedirectToAction(nameof(Computers));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thêm máy tính: {Message}", ex.Message);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi thêm máy tính: " + ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("ModelState không hợp lệ: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }
            
            var categories = _context.ComputerCategories.ToList();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "ID", "Name");
            return View(computer);
        }

        // GET: Admin/Computers/Edit/5
        public async Task<IActionResult> EditComputer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ComputerCategories, "ID", "Name");
            return View(computer);
        }

        // POST: Admin/Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComputer(int id, Computer computer)
        {
            if (id != computer.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(computer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComputerExists(computer.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Computers));
            }
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ComputerCategories, "ID", "Name");
            return View(computer);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.ID == id);
        }

        private bool ComputerExists(int id)
        {
            return _context.Computers.Any(e => e.ID == id);
        }

        // GET: Admin/ComputerCategories
        public async Task<IActionResult> ComputerCategories()
        {
            var categories = await _context.ComputerCategories.ToListAsync();
            return View(categories);
        }

        // GET: Admin/ComputerCategories/Create
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: Admin/ComputerCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(ComputerCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ComputerCategories));
            }
            return View(category);
        }

        // GET: Admin/ComputerCategories/Edit/5
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.ComputerCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/ComputerCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, ComputerCategory category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComputerCategoryExists(category.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ComputerCategories));
            }
            return View(category);
        }

        private bool ComputerCategoryExists(int id)
        {
            return _context.ComputerCategories.Any(e => e.ID == id);
        }

        // GET: Admin/Services
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        // GET: Admin/Recharge
        public async Task<IActionResult> Recharge()
        {
            var requests = await _context.RechargeRequests
                .Include(r => r.User)
                .Select(r => new
                {
                    r.ID,
                    r.UserID,
                    r.User,
                    r.Amount,
                    r.RequestTime,
                    r.ProcessedTime,
                    r.Status,
                    r.Note
                })
                .OrderByDescending(r => r.RequestTime)
                .ToListAsync();

            return View(requests.Select(r => new RechargeRequest
            {
                ID = r.ID,
                UserID = r.UserID,
                User = r.User,
                Amount = r.Amount,
                RequestTime = r.RequestTime,
                ProcessedTime = r.ProcessedTime,
                Status = r.Status,
                Note = r.Note
            }).ToList());
        }

        // POST: Admin/ProcessRecharge
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRecharge(int id, string action, string? note = null)
        {
            var request = await _context.RechargeRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (request == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu nạp tiền";
                return RedirectToAction("Recharge");
            }

            if (request.Status != RechargeStatus.Pending)
            {
                TempData["Error"] = "Yêu cầu này đã được xử lý";
                return RedirectToAction("Recharge");
            }

            bool isApproved = action.ToLower() == "approve";
            request.Status = isApproved ? RechargeStatus.Approved : RechargeStatus.Rejected;
            request.ProcessedTime = DateTime.Now;
            request.Note = note;

            if (isApproved)
            {
                // Cập nhật số dư người dùng
                request.User.Balance += request.Amount;
                _context.Users.Update(request.User);

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
                        TotalServiceRevenue = 0,
                        TotalRecharge = request.Amount
                    };
                    _context.DailyRevenues.Add(dailyRevenue);
                }
                else
                {
                    dailyRevenue.TotalRecharge += request.Amount;
                    _context.DailyRevenues.Update(dailyRevenue);
                }

                // Tạo thông báo cho người dùng
                var notification = new Notification
                {
                    UserID = request.UserID,
                    User = request.User,
                    Title = "Nạp tiền thành công",
                    Content = $"Yêu cầu nạp {request.Amount:N0} VNĐ của bạn đã được phê duyệt",
                    CreatedTime = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }
            else
            {
                // Tạo thông báo từ chối cho người dùng
                var notification = new Notification
                {
                    UserID = request.UserID,
                    User = request.User,
                    Title = "Nạp tiền bị từ chối",
                    Content = $"Yêu cầu nạp {request.Amount:N0} VNĐ của bạn bị từ chối. Lý do: {note ?? "Không có"}",
                    CreatedTime = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = isApproved 
                    ? $"Đã phê duyệt yêu cầu nạp {request.Amount:N0} VNĐ cho {request.User.Username}"
                    : $"Đã từ chối yêu cầu nạp {request.Amount:N0} VNĐ của {request.User.Username}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xử lý yêu cầu: " + ex.Message;
            }

            return RedirectToAction("Recharge");
        }

        // GET: Admin/Revenue
        [HttpGet]
        public async Task<IActionResult> Revenue(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.DailyRevenues.AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(r => r.ReportDate >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(r => r.ReportDate <= toDate.Value.Date);
                }

                var revenues = await query
                    .OrderByDescending(r => r.ReportDate)
                    .ToListAsync();

                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                return View(revenues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu doanh thu");
                TempData["Error"] = "Có lỗi xảy ra khi lấy dữ liệu doanh thu";
                return View(new List<DailyRevenue>());
            }
        }

        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                var now = DateTime.Now;
                var sessions = await _context.UsageSessions
                    .Include(s => s.User)
                    .Include(s => s.Computer)
                        .ThenInclude(c => c.Category)
                    .Where(s => s.EndTime == null)
                    .Select(s => new
                    {
                        s.ID,
                        userName = s.User.Username,
                        computerName = s.Computer.Name,
                        categoryName = s.Computer.Category.Name,
                        startTime = s.StartTime,
                        pricePerHour = s.Computer.Category.PricePerHour
                    })
                    .ToListAsync();

                var activeSessions = sessions.Select(s => new
                {
                    s.ID,
                    s.userName,
                    s.computerName,
                    s.categoryName,
                    s.startTime,
                    duration = (int)(now - s.startTime).TotalMinutes,
                    cost = Math.Round((decimal)(now - s.startTime).TotalHours * s.pricePerHour, 0)
                }).ToList();

                _logger.LogInformation("Đã lấy được {0} phiên sử dụng đang hoạt động", activeSessions.Count);
                return Json(activeSessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiên sử dụng đang hoạt động");
                return Json(new { error = "Đã xảy ra lỗi khi lấy dữ liệu: " + ex.Message });
            }
        }

        public IActionResult ActiveSessions()
        {
            return View();
        }
    }
} 