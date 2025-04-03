// Tạo file: Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using HeThongTimViec.Data;
using Microsoft.EntityFrameworkCore;

namespace HeThongTimViec.Controllers
{
    public class UsersController : Controller
    {
        private readonly HeThongTimViecContext _context;

        public UsersController(HeThongTimViecContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
    }
}