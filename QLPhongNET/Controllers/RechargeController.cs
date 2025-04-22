using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;

namespace QLPhongNET.Controllers
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

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("Username") != null;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsLoggedIn())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            base.OnActionExecuting(context);
        }

        // GET: Recharge/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Recharge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(decimal amount)
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

            if (amount < 1000)
            {
                ModelState.AddModelError("Amount", "Số tiền nạp tối thiểu là 1.000 VNĐ");
                return View();
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
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yêu cầu nạp tiền đã được gửi. Vui lòng chờ admin phê duyệt.";
            return RedirectToAction("Index", "User");
        }

        // GET: Recharge/History
        public async Task<IActionResult> History()
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

            var requests = await _context.RechargeRequests
                .Where(r => r.UserID == user.ID)
                .OrderByDescending(r => r.RequestTime)
                .ToListAsync();

            return View(requests);
        }
    }
} 