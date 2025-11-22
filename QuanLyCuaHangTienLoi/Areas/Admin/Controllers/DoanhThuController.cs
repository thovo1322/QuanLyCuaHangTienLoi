using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DoanhThuController : Controller
    {
        private readonly QlchtlContext _context;

        public DoanhThuController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Admin/DoanhThu
        public async Task<IActionResult> Index()
        {
            var revenueData = new DoanhThuViewModel
            {
                TotalRevenue = await _context.DonHangs.SumAsync(d => d.TongTienSauGiam),
                TotalOrders = await _context.DonHangs.CountAsync(),
                AverageOrderValue = await _context.DonHangs.AverageAsync(d => d.TongTienSauGiam),
                TodayRevenue = await _context.DonHangs
                    .Where(d => d.NgayTao.HasValue && d.NgayTao.Value.Date == DateTime.Today)
                    .SumAsync(d => d.TongTienSauGiam),
                ThisMonthRevenue = await _context.DonHangs
                    .Where(d => d.NgayTao.HasValue && d.NgayTao.Value.Month == DateTime.Now.Month && d.NgayTao.Value.Year == DateTime.Now.Year)
                    .SumAsync(d => d.TongTienSauGiam),
                RevenueByMonth = await GetRevenueByMonth(),
                TopProducts = await GetTopProducts()
            };

            return View(revenueData);
        }

        // GET: Admin/DoanhThu/Details
        public async Task<IActionResult> Details(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.DonHangs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(d => d.NgayTao >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(d => d.NgayTao <= endDate.Value);

            var orders = await query
                .Include(d => d.FkMaNvNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.FkMaSanPhamNavigation)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(orders);
        }

        private async Task<List<DoanhThuTheoThang>> GetRevenueByMonth()
        {
            var currentYear = DateTime.Now.Year;
            var monthlyRevenues = new List<DoanhThuTheoThang>();

            for (int month = 1; month <= 12; month++)
            {
                var revenue = await _context.DonHangs
                    .Where(d => d.NgayTao.HasValue && d.NgayTao.Value.Month == month && d.NgayTao.Value.Year == currentYear)
                    .SumAsync(d => d.TongTienSauGiam);

                monthlyRevenues.Add(new DoanhThuTheoThang
                {
                    Month = month,
                    MonthName = GetMonthName(month),
                    Revenue = revenue
                });
            }

            return monthlyRevenues;
        }

        private async Task<List<SanPhamBanChay>> GetTopProducts()
        {
            var topProducts = await _context.ChiTietDonHangs
                .Include(c => c.FkMaSanPhamNavigation)
                .GroupBy(c => new { c.FkMaSanPham, c.FkMaSanPhamNavigation.Ten })
                .Select(g => new SanPhamBanChay
                {
                    ProductName = g.Key.Ten,
                    TotalQuantity = g.Sum(c => c.SoLuong),
                    TotalRevenue = g.Sum(c => c.GiaBanLucDo * c.SoLuong)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(10)
                .ToListAsync();

            return topProducts;
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Tháng 1",
                2 => "Tháng 2",
                3 => "Tháng 3",
                4 => "Tháng 4",
                5 => "Tháng 5",
                6 => "Tháng 6",
                7 => "Tháng 7",
                8 => "Tháng 8",
                9 => "Tháng 9",
                10 => "Tháng 10",
                11 => "Tháng 11",
                12 => "Tháng 12",
                _ => "Không xác định"
            };
        }
    }

    public class DoanhThuViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public List<DoanhThuTheoThang> RevenueByMonth { get; set; } = new List<DoanhThuTheoThang>();
        public List<SanPhamBanChay> TopProducts { get; set; } = new List<SanPhamBanChay>();
    }

    public class DoanhThuTheoThang
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class SanPhamBanChay
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
