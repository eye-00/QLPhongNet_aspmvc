using Microsoft.AspNetCore.Mvc;
using QLPhongNET.Data;
using QLPhongNET.Models;

namespace QLPhongNET.Controllers
{
    public class InitController : Controller
    {
        private readonly QLPhongNetContext _context;

        public InitController(QLPhongNetContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Thêm danh mục máy
            var categories = new List<ComputerCategory>
            {
                new ComputerCategory { Name = "Phổ thông", PricePerHour = 10000 },
                new ComputerCategory { Name = "Cao cấp", PricePerHour = 20000 }
            };
            _context.ComputerCategories.AddRange(categories);
            _context.SaveChanges();

            // Thêm máy tính
            var computers = new List<Computer>
            {
                new Computer { Name = "Máy 1", Status = ComputerStatus.Available, CatID = 1 },
                new Computer { Name = "Máy 2", Status = ComputerStatus.Available, CatID = 1 },
                new Computer { Name = "Máy VIP 1", Status = ComputerStatus.Available, CatID = 2 },
                new Computer { Name = "Máy VIP 2", Status = ComputerStatus.Available, CatID = 2 }
            };
            _context.Computers.AddRange(computers);
            _context.SaveChanges();

            // Thêm người dùng
            var users = new List<User>
            {
                new User { Username = "user1", Password = "123456", FullName = "Nguyễn Văn A", Phone = "0912345678", Balance = 50000, Role = UserRole.User },
                new User { Username = "user2", Password = "123456", FullName = "Nguyễn Thị B", Phone = "0987654321", Balance = 200000, Role = UserRole.User },
                new User { Username = "admin1", Password = "Admin1234456", FullName = "Trần Thị C", Phone = "0123123123", Balance = 0, Role = UserRole.Admin }
            };
            _context.Users.AddRange(users);
            _context.SaveChanges();

            // Thêm dịch vụ
            var services = new List<Service>
            {
                new Service { Name = "Mì tôm", Price = 15000, Description = "Mì tôm thường" },
                new Service { Name = "Coca Cola", Price = 10000, Description = "Nước giải khát" },
                new Service { Name = "Bánh mì", Price = 20000, Description = "Bánh mì thịt" }
            };
            _context.Services.AddRange(services);
            _context.SaveChanges();

            return Content("Đã khởi tạo dữ liệu mẫu thành công!");
        }
    }
} 