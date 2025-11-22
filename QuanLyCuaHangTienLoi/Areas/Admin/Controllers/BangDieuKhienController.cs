using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BangDieuKhienController : Controller
    {
        private readonly QlchtlContext _context;

        public BangDieuKhienController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Admin/BangDieuKhien
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap");
            }

            // Lấy thống kê tổng quan
            var dashboardData = new BangDieuKhienViewModel
            {
                TotalOrders = await _context.DonHangs.CountAsync(),
                TotalProducts = await _context.SanPhams.CountAsync(),
                TotalRevenue = await _context.DonHangs.SumAsync(d => d.TongTienSauGiam),
                RecentOrders = await _context.DonHangs
                    .Include(d => d.FkMaNvNavigation)
                    .OrderByDescending(d => d.NgayTao)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboardData);
        }
    }

    public class BangDieuKhienViewModel
    {
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DonHang> RecentOrders { get; set; } = new List<DonHang>();
    }
}
