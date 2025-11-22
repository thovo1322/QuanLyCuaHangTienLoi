using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoaiSanPhamController : Controller
    {

        private readonly QlchtlContext _context;

        public LoaiSanPhamController(QlchtlContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var loaiSanPhams = await _context.LoaiSanPhams
                                     .Include(l => l.MaDanhMucNavigation)
                                     .ToListAsync();

            return View(loaiSanPhams);
        }
        

    }
}
