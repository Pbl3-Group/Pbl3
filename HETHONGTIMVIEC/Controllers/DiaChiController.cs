// File: Controllers/DiaChiController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DiaChiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public DiaChiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuanHuyenByThanhPho(int id)
        {
            var quanHuyens = await _context.QuanHuyens
                .Where(qh => qh.ThanhPhoId == id)
                .OrderBy(qh => qh.Ten)
                .Select(qh => new { id = qh.Id, ten = qh.Ten })
                .ToListAsync();
                
            return Ok(quanHuyens);
        }
    }
}