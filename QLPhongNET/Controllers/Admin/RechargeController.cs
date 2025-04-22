using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;

namespace QLPhongNET.Controllers.Admin
{
    public class RechargeController : Controller
    {
        private readonly QLPhongNetContext _context;
        private readonly ILogger<RechargeController> _logger;

        public RechargeController(QLPhongNetContext context, ILogger<RechargeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Recharge
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.Role != UserRole.Admin)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _context.RechargeRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.RequestTime)
                .ToListAsync();

            return View(requests);
        }

        // GET: Admin/Recharge/Process/5
        public async Task<IActionResult> Process(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.Role != UserRole.Admin)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.RechargeRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (request == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu nạp tiền";
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }

        // POST: Admin/Recharge/Process/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int id, bool approve, string? note)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.Role != UserRole.Admin)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.RechargeRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (request == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu nạp tiền";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != RechargeStatus.Pending)
            {
                TempData["Error"] = "Yêu cầu này đã được xử lý";
                return RedirectToAction(nameof(Index));
            }

            if (approve)
            {
                request.User.Balance += request.Amount;
                request.Status = RechargeStatus.Approved;
                request.ProcessedTime = DateTime.Now;
                request.Note = note;

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
                        TotalRecharge = 0
                    };
                    _context.DailyRevenues.Add(dailyRevenue);
                }

                dailyRevenue.TotalRecharge += request.Amount;
                request.DailyRevenueID = dailyRevenue.ID;

                TempData["Success"] = "Đã duyệt yêu cầu nạp tiền thành công";
            }
            else
            {
                request.Status = RechargeStatus.Rejected;
                request.ProcessedTime = DateTime.Now;
                request.Note = note;

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

                TempData["Success"] = "Đã từ chối yêu cầu nạp tiền";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 