using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;
using Microsoft.AspNetCore.Http;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyTonKhoController : Controller
    {
        private readonly QlchtlContext _context;

        public QuanLyTonKhoController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Admin/QuanLyTonKho - Xem danh sách hàng tồn kho
        public async Task<IActionResult> Index(string? search, int? minStock)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            // Đồng bộ hóa: Tạo bản ghi tồn kho cho các sản phẩm chưa có
            await DongBoTonKho();

            var query = _context.SanPhams
                .Include(s => s.FkMaDanhMucNavigation)
                .Include(s => s.MaLoaiConNavigation)
                .Where(s => s.TrangThai == "Hoạt động" || s.TrangThai == "Còn hàng")
                .AsQueryable();

            // Tìm kiếm theo tên hoặc mã sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int maSanPham))
                {
                    query = query.Where(s => s.MaSanPham == maSanPham);
                }
                else
                {
                    query = query.Where(s => s.Ten.Contains(search) || s.MaVach.Contains(search));
                }
            }

            var sanPhams = await query.OrderBy(s => s.Ten).ToListAsync();

            // Tính toán thông tin tồn kho và cảnh báo
            var tonKhoList = new List<TonKhoViewModel>();
            var ngayHienTai = DateTime.Now;

            foreach (var sp in sanPhams)
            {
                // Lấy tồn kho từ bảng TonKho hoặc dùng SoLuong từ SanPham
                var tonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(t => t.MaSanPham == sp.MaSanPham);

                var soLuongTon = tonKho?.SoLuongHienTai ?? sp.SoLuong ?? 0;

                // Lấy giao dịch kho gần nhất
                var giaoDichGanNhat = await _context.GiaoDichKhos
                    .Where(g => g.FkMaSanPham == sp.MaSanPham)
                    .OrderByDescending(g => g.NgayTao)
                    .FirstOrDefaultAsync();

                // Tính toán cảnh báo
                var canhBao = "";
                var mauCanhBao = "";

                // Cảnh báo sắp hết hàng (dưới 10 sản phẩm)
                if (soLuongTon < 10)
                {
                    canhBao = "Sắp hết hàng";
                    mauCanhBao = "danger";
                }
                // Cảnh báo hàng ít (dưới 50 sản phẩm)
                else if (soLuongTon < 50)
                {
                    canhBao = "Hàng ít";
                    mauCanhBao = "warning";
                }

                // Cảnh báo sắp hết hạn (nếu có thông tin hạn sử dụng)
                // Giả sử có trường NgayHetHan trong SanPham hoặc TonKho
                // Nếu không có thì bỏ qua phần này

                tonKhoList.Add(new TonKhoViewModel
                {
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.Ten,
                    MaVach = sp.MaVach,
                    SoLuongTon = soLuongTon,
                    DonVi = "Pcs",
                    GiaVon = sp.GiaVon,
                    GiaBan = sp.GiaBan,
                    NgayCapNhatCuoi = tonKho?.NgayCapNhatCuoi ?? giaoDichGanNhat?.NgayTao,
                    CanhBao = canhBao,
                    MauCanhBao = mauCanhBao,
                    DanhMuc = sp.FkMaDanhMucNavigation?.Ten ?? "",
                    LoaiSanPham = sp.MaLoaiConNavigation?.TenLoai ?? ""
                });
            }

            // Lọc theo số lượng tối thiểu nếu có
            if (minStock.HasValue)
            {
                tonKhoList = tonKhoList.Where(t => t.SoLuongTon < minStock.Value).ToList();
            }

            ViewBag.Search = search;
            ViewBag.MinStock = minStock;
            ViewBag.TongSoSanPham = tonKhoList.Count;
            ViewBag.SoSanPhamCanhBao = tonKhoList.Count(t => !string.IsNullOrEmpty(t.CanhBao));

            return View(tonKhoList);
        }

        // GET: Admin/QuanLyTonKho/KiemKho
        public async Task<IActionResult> KiemKho(int? maSanPham)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var adminId = HttpContext.Session.GetInt32("AdminId");

            if (maSanPham.HasValue)
            {
                var sanPham = await _context.SanPhams
                    .Include(s => s.FkMaDanhMucNavigation)
                    .Include(s => s.MaLoaiConNavigation)
                    .FirstOrDefaultAsync(s => s.MaSanPham == maSanPham);

                if (sanPham == null)
                {
                    return NotFound();
                }

                // Lấy tồn kho hiện tại
                var tonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(t => t.MaSanPham == maSanPham);

                ViewBag.SanPham = sanPham;
                ViewBag.TonKhoHienTai = tonKho?.SoLuongHienTai ?? sanPham.SoLuong ?? 0;
                ViewBag.AdminId = adminId;
            }

            // Lấy danh sách sản phẩm để chọn
            var sanPhams = await _context.SanPhams
                .Where(s => s.TrangThai == "Hoạt động")
                .OrderBy(s => s.Ten)
                .ToListAsync();

            ViewBag.SanPhams = sanPhams;

            return View();
        }

        // POST: Admin/QuanLyTonKho/KiemKho
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KiemKho(int maSanPham, int soLuongThucTe, string? lyDo)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var adminId = HttpContext.Session.GetInt32("AdminId");

            var sanPham = await _context.SanPhams.FindAsync(maSanPham);
            if (sanPham == null)
            {
                return NotFound();
            }

            // Lấy tồn kho hiện tại
            var tonKho = await _context.TonKhos
                .FirstOrDefaultAsync(t => t.MaSanPham == maSanPham);

            var soLuongCu = tonKho?.SoLuongHienTai ?? sanPham.SoLuong ?? 0;
            var chenhLech = soLuongThucTe - soLuongCu;

            // Cập nhật hoặc tạo mới TonKho
            if (tonKho == null)
            {
                // Lấy IdTonKho lớn nhất và cộng thêm 1
                var maxId = await _context.TonKhos
                    .Select(t => t.IdTonKho)
                    .DefaultIfEmpty(0)
                    .MaxAsync();
                
                tonKho = new TonKho
                {
                    IdTonKho = maxId + 1,
                    MaSanPham = maSanPham,
                    SoLuongHienTai = soLuongThucTe,
                    NgayCapNhatCuoi = DateTime.Now
                };
                _context.TonKhos.Add(tonKho);
            }
            else
            {
                tonKho.SoLuongHienTai = soLuongThucTe;
                tonKho.NgayCapNhatCuoi = DateTime.Now;
            }

            // Cập nhật SoLuong trong SanPham
            sanPham.SoLuong = soLuongThucTe;

            // Tạo giao dịch kho nếu có chênh lệch
            if (chenhLech != 0)
            {
                var giaoDich = new GiaoDichKho
                {
                    LoaiGiaoDich = chenhLech > 0 ? "Nhập kho (Kiểm kho)" : "Xuất kho (Kiểm kho)",
                    SoLuong = Math.Abs(chenhLech),
                    NgayTao = DateTime.Now,
                    TrangThai = "Hoàn thành",
                    FkMaSanPham = maSanPham
                };
                _context.GiaoDichKhos.Add(giaoDich);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật tồn kho cho sản phẩm {sanPham.Ten}. Số lượng: {soLuongCu} → {soLuongThucTe}";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/QuanLyTonKho/CapNhatTonKho
        public async Task<IActionResult> CapNhatTonKho(int? maSanPham)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var adminId = HttpContext.Session.GetInt32("AdminId");

            if (maSanPham.HasValue)
            {
                var sanPham = await _context.SanPhams
                    .Include(s => s.FkMaDanhMucNavigation)
                    .Include(s => s.MaLoaiConNavigation)
                    .FirstOrDefaultAsync(s => s.MaSanPham == maSanPham);

                if (sanPham == null)
                {
                    return NotFound();
                }

                // Lấy tồn kho hiện tại
                var tonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(t => t.MaSanPham == maSanPham);

                // Lấy lịch sử giao dịch
                var lichSuGiaoDich = await _context.GiaoDichKhos
                    .Where(g => g.FkMaSanPham == maSanPham)
                    .OrderByDescending(g => g.NgayTao)
                    .Take(10)
                    .ToListAsync();

                ViewBag.SanPham = sanPham;
                ViewBag.TonKhoHienTai = tonKho?.SoLuongHienTai ?? sanPham.SoLuong ?? 0;
                ViewBag.LichSuGiaoDich = lichSuGiaoDich;
                ViewBag.AdminId = adminId;
            }

            // Lấy danh sách sản phẩm để chọn
            var sanPhams = await _context.SanPhams
                .Where(s => s.TrangThai == "Hoạt động")
                .OrderBy(s => s.Ten)
                .ToListAsync();

            ViewBag.SanPhams = sanPhams;

            return View();
        }

        // POST: Admin/QuanLyTonKho/CapNhatTonKho
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTonKho(int maSanPham, string loaiGiaoDich, int soLuong, string? ghiChu)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var adminId = HttpContext.Session.GetInt32("AdminId");

            var sanPham = await _context.SanPhams.FindAsync(maSanPham);
            if (sanPham == null)
            {
                return NotFound();
            }

            // Lấy tồn kho hiện tại
            var tonKho = await _context.TonKhos
                .FirstOrDefaultAsync(t => t.MaSanPham == maSanPham);

            var soLuongCu = tonKho?.SoLuongHienTai ?? sanPham.SoLuong ?? 0;
            int soLuongMoi;

            // Tính số lượng mới dựa trên loại giao dịch
            if (loaiGiaoDich == "Nhập kho")
            {
                soLuongMoi = soLuongCu + soLuong;
            }
            else if (loaiGiaoDich == "Xuất kho")
            {
                if (soLuong > soLuongCu)
                {
                    TempData["Error"] = $"Số lượng xuất ({soLuong}) vượt quá số lượng tồn kho hiện tại ({soLuongCu})";
                    return RedirectToAction(nameof(CapNhatTonKho), new { maSanPham = maSanPham });
                }
                soLuongMoi = soLuongCu - soLuong;
            }
            else
            {
                TempData["Error"] = "Loại giao dịch không hợp lệ";
                return RedirectToAction(nameof(CapNhatTonKho), new { maSanPham = maSanPham });
            }

            // Cập nhật hoặc tạo mới TonKho
            if (tonKho == null)
            {
                // Lấy IdTonKho lớn nhất và cộng thêm 1
                var maxId = await _context.TonKhos
                    .Select(t => t.IdTonKho)
                    .DefaultIfEmpty(0)
                    .MaxAsync();
                
                tonKho = new TonKho
                {
                    IdTonKho = maxId + 1,
                    MaSanPham = maSanPham,
                    SoLuongHienTai = soLuongMoi,
                    NgayCapNhatCuoi = DateTime.Now
                };
                _context.TonKhos.Add(tonKho);
            }
            else
            {
                tonKho.SoLuongHienTai = soLuongMoi;
                tonKho.NgayCapNhatCuoi = DateTime.Now;
            }

            // Cập nhật SoLuong trong SanPham
            sanPham.SoLuong = soLuongMoi;

            // Tạo giao dịch kho
            var giaoDich = new GiaoDichKho
            {
                LoaiGiaoDich = loaiGiaoDich + (string.IsNullOrEmpty(ghiChu) ? "" : $" - {ghiChu}"),
                SoLuong = soLuong,
                NgayTao = DateTime.Now,
                TrangThai = "Hoàn thành",
                FkMaSanPham = maSanPham
            };
            _context.GiaoDichKhos.Add(giaoDich);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã {loaiGiaoDich.ToLower()} {soLuong} sản phẩm {sanPham.Ten}. Tồn kho: {soLuongCu} → {soLuongMoi}";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/QuanLyTonKho/HuyHang
        public async Task<IActionResult> HuyHang()
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            // Lấy danh sách các giao dịch hủy hàng
            var danhSachHuyHang = await _context.GiaoDichKhos
                .Where(g => g.LoaiGiaoDich != null && g.LoaiGiaoDich.Contains("Hủy hàng"))
                .Include(g => g.FkMaSanPhamNavigation)
                .OrderByDescending(g => g.NgayTao ?? DateTime.MinValue)
                .ToListAsync();

            ViewBag.DanhSachHuyHang = danhSachHuyHang;

            return View();
        }

        // API: Tìm sản phẩm theo barcode
        [HttpGet]
        public async Task<IActionResult> TimSanPhamTheoBarcode(string barcode)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            // Kiểm tra độ dài barcode (6, 8, hoặc 13 số)
            if (string.IsNullOrEmpty(barcode) || (!barcode.All(char.IsDigit)) || 
                (barcode.Length != 6 && barcode.Length != 8 && barcode.Length != 13))
            {
                return Json(new { success = false, message = "Mã vạch phải là 6, 8 hoặc 13 số" });
            }

            // Tìm sản phẩm theo mã vạch
            var sanPham = await _context.SanPhams
                .Include(s => s.FkMaDanhMucNavigation)
                .Include(s => s.MaLoaiConNavigation)
                .FirstOrDefaultAsync(s => s.MaVach == barcode);

            if (sanPham == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm với mã vạch này" });
            }

            // Lấy tồn kho hiện tại
            var tonKho = await _context.TonKhos
                .FirstOrDefaultAsync(t => t.MaSanPham == sanPham.MaSanPham);

            var soLuongTon = tonKho?.SoLuongHienTai ?? sanPham.SoLuong ?? 0;

            return Json(new
            {
                success = true,
                sanPham = new
                {
                    maSanPham = sanPham.MaSanPham,
                    ten = sanPham.Ten,
                    maVach = sanPham.MaVach,
                    danhMuc = sanPham.FkMaDanhMucNavigation?.Ten ?? "",
                    loaiSanPham = sanPham.MaLoaiConNavigation?.TenLoai ?? "",
                    soLuongTon = soLuongTon
                }
            });
        }

        // POST: Admin/QuanLyTonKho/HuyHang
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyHang(int maSanPham, int soLuongHuy)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var adminId = HttpContext.Session.GetInt32("AdminId");

            var sanPham = await _context.SanPhams.FindAsync(maSanPham);
            if (sanPham == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm";
                return View();
            }

            // Lấy tồn kho hiện tại
            var tonKho = await _context.TonKhos
                .FirstOrDefaultAsync(t => t.MaSanPham == maSanPham);

            var soLuongCu = tonKho?.SoLuongHienTai ?? sanPham.SoLuong ?? 0;

            // Kiểm tra số lượng hủy
            if (soLuongHuy <= 0)
            {
                TempData["Error"] = "Số lượng hủy phải lớn hơn 0";
                return View();
            }

            if (soLuongHuy > soLuongCu)
            {
                TempData["Error"] = $"Số lượng hủy ({soLuongHuy}) vượt quá số lượng tồn kho hiện tại ({soLuongCu})";
                return View();
            }

            var soLuongMoi = soLuongCu - soLuongHuy;

            // Cập nhật hoặc tạo mới TonKho
            if (tonKho == null)
            {
                // Lấy IdTonKho lớn nhất và cộng thêm 1
                var maxId = await _context.TonKhos
                    .Select(t => t.IdTonKho)
                    .DefaultIfEmpty(0)
                    .MaxAsync();
                
                tonKho = new TonKho
                {
                    IdTonKho = maxId + 1,
                    MaSanPham = maSanPham,
                    SoLuongHienTai = soLuongMoi,
                    NgayCapNhatCuoi = DateTime.Now
                };
                _context.TonKhos.Add(tonKho);
            }
            else
            {
                tonKho.SoLuongHienTai = soLuongMoi;
                tonKho.NgayCapNhatCuoi = DateTime.Now;
            }

            // Cập nhật SoLuong trong SanPham
            sanPham.SoLuong = soLuongMoi;

            // Tạo giao dịch kho với ghi chú "Hủy hàng"
            var giaoDich = new GiaoDichKho
            {
                LoaiGiaoDich = "Xuất kho - Hủy hàng",
                SoLuong = soLuongHuy,
                NgayTao = DateTime.Now,
                TrangThai = "Hoàn thành",
                FkMaSanPham = maSanPham
            };
            _context.GiaoDichKhos.Add(giaoDich);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã hủy {soLuongHuy} sản phẩm {sanPham.Ten}. Tồn kho: {soLuongCu} → {soLuongMoi}";
            return RedirectToAction(nameof(HuyHang));
        }

        // Đồng bộ hóa: Tạo bản ghi tồn kho cho các sản phẩm chưa có
        private async Task DongBoTonKho()
        {
            // Lấy tất cả sản phẩm
            var allSanPhams = await _context.SanPhams.ToListAsync();
            
            // Lấy tất cả bản ghi tồn kho hiện có
            var allTonKhos = await _context.TonKhos.ToListAsync();
            var maSanPhamDaCoTonKho = allTonKhos.Select(t => t.MaSanPham).Where(m => m.HasValue).Select(m => m.Value).ToHashSet();
            
            // Tìm các sản phẩm chưa có bản ghi tồn kho
            var sanPhamChuaCoTonKho = allSanPhams
                .Where(sp => !maSanPhamDaCoTonKho.Contains(sp.MaSanPham))
                .ToList();
            
            if (sanPhamChuaCoTonKho.Any())
            {
                // Lấy IdTonKho lớn nhất
                var maxId = allTonKhos.Any() 
                    ? allTonKhos.Max(t => t.IdTonKho) 
                    : 0;
                
                // Tạo bản ghi tồn kho cho các sản phẩm chưa có
                foreach (var sp in sanPhamChuaCoTonKho)
                {
                    maxId++;
                    var tonKho = new TonKho
                    {
                        IdTonKho = maxId,
                        MaSanPham = sp.MaSanPham,
                        SoLuongHienTai = sp.SoLuong ?? 0,
                        NgayCapNhatCuoi = DateTime.Now
                    };
                    _context.TonKhos.Add(tonKho);
                }
                
                await _context.SaveChangesAsync();
            }
        }
    }

    // ViewModel cho tồn kho
    public class TonKhoViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? MaVach { get; set; }
        public int SoLuongTon { get; set; }
        public string DonVi { get; set; } = "Pcs";
        public decimal GiaVon { get; set; }
        public decimal GiaBan { get; set; }
        public DateTime? NgayCapNhatCuoi { get; set; }
        public string CanhBao { get; set; } = string.Empty;
        public string MauCanhBao { get; set; } = string.Empty;
        public string DanhMuc { get; set; } = string.Empty;
        public string LoaiSanPham { get; set; } = string.Empty;
    }
}

