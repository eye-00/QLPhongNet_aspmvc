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
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
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
            ViewBag.ComputersInUse = await _context.Computers.CountAsync(c => c.Status == "In Use");
            ViewBag.ComputersMaintenance = await _context.Computers.CountAsync(c => c.Status == "Maintenance");
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
        public async Task<IActionResult> EditUser(int id, User user)
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
                return RedirectToAction(nameof(Users));
            }
            return View(user);
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
                    if (string.IsNullOrEmpty(computer.Status))
                    {
                        computer.Status = "Available";
                    }
                    
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
    }
} 