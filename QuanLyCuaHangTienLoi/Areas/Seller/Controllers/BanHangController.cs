using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

namespace QuanLyCuaHangTienLoi.Areas.Seller.Controllers
{
    [Area("Seller")]
    public class BanHangController : Controller
    {
        private readonly QlchtlContext _context;

        public BanHangController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Seller/BanHang
        public async Task<IActionResult> Index(int? maDanhMuc)
        {
            // Kiểm tra đăng nhập nhân viên
            if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            var employeeName = HttpContext.Session.GetString("EmployeeName");

            // Lấy thông tin nhân viên
            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(n => n.MaNv == employeeId);

            ViewData["EmployeeName"] = employeeName ?? "Nhân viên";
            ViewData["EmployeeId"] = employeeId;
            ViewData["CurrentTime"] = DateTime.Now;

            // Lấy danh sách danh mục
            var danhMucs = await _context.DanhMucs
                .Where(d => d.TrangThai == "Hoạt động")
                .ToListAsync();

            ViewData["DanhMucs"] = danhMucs;

            // Lấy sản phẩm theo danh mục (mặc định lấy danh mục đầu tiên nếu không chọn)
            if (!maDanhMuc.HasValue && danhMucs.Any())
            {
                maDanhMuc = danhMucs.First().MaDanhMuc;
            }

            var sanPhams = new List<SanPham>();
            if (maDanhMuc.HasValue)
            {
                sanPhams = await _context.SanPhams
                    .Where(s => s.FkMaDanhMuc == maDanhMuc && s.TrangThai == "Còn hàng")
                    .Include(s => s.MaLoaiConNavigation)
                    .OrderBy(s => s.Ten)
                    .ToListAsync();
            }

            ViewData["MaDanhMuc"] = maDanhMuc;
            ViewData["SanPhams"] = sanPhams;

            return View();
        }

        // API: Lấy sản phẩm theo danh mục
        [HttpGet]
        public async Task<IActionResult> GetSanPhamByDanhMuc(int maDanhMuc)
        {
            var sanPhams = await _context.SanPhams
                .Where(s => s.FkMaDanhMuc == maDanhMuc && s.TrangThai == "Còn hàng")
                .Include(s => s.MaLoaiConNavigation)
                .OrderBy(s => s.Ten)
                .Select(s => new
                {
                    MaSanPham = s.MaSanPham,
                    Ten = s.Ten,
                    GiaBan = s.GiaBan,
                    SoLuong = s.SoLuong,
                    MaVach = s.MaVach
                })
                .ToListAsync();

            return Json(sanPhams);
        }

        // API: Lấy thông tin sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetSanPhamInfo(int maSanPham)
        {
            var sanPham = await _context.SanPhams
                .Where(s => s.MaSanPham == maSanPham)
                .Select(s => new
                {
                    MaSanPham = s.MaSanPham,
                    Ten = s.Ten,
                    GiaBan = s.GiaBan,
                    SoLuong = s.SoLuong,
                    MaVach = s.MaVach
                })
                .FirstOrDefaultAsync();

            if (sanPham == null)
            {
                return NotFound();
            }

            return Json(sanPham);
        }

        // API: Tìm sản phẩm theo mã vạch
        [HttpGet]
        public async Task<IActionResult> GetSanPhamByMaVach(string maVach)
        {
            if (string.IsNullOrEmpty(maVach))
            {
                return BadRequest(new { success = false, message = "Mã vạch không được để trống" });
            }

            var sanPham = await _context.SanPhams
                .Where(s => s.MaVach == maVach && s.TrangThai == "Còn hàng")
                .Select(s => new
                {
                    maSanPham = s.MaSanPham,
                    ten = s.Ten,
                    giaBan = s.GiaBan,
                    soLuong = s.SoLuong,
                    maVach = s.MaVach
                })
                .FirstOrDefaultAsync();

            if (sanPham == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm với mã vạch này hoặc sản phẩm đã hết hàng" });
            }

            return Json(new { success = true, data = sanPham });
        }

        // GET: Seller/BanHang/KiemTraSanPham
        public async Task<IActionResult> KiemTraSanPham(string search, int? maDanhMuc, int? maLoaiCon)
        {
            // Kiểm tra đăng nhập nhân viên
            if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            // Lấy danh sách danh mục để hiển thị trong dropdown
            var danhMucs = await _context.DanhMucs
                .Where(d => d.TrangThai == "Hoạt động" || d.TrangThai == "Còn hàng")
                .OrderBy(d => d.Ten)
                .ToListAsync();

            ViewData["DanhMucs"] = danhMucs;

            // Lấy danh sách loại sản phẩm để hiển thị trong dropdown
            var loaiSanPhams = await _context.LoaiSanPhams
                .OrderBy(l => l.TenLoai)
                .ToListAsync();

            ViewData["LoaiSanPhams"] = loaiSanPhams;

            // Query sản phẩm
            var query = _context.SanPhams
                .Include(s => s.FkMaDanhMucNavigation)
                .Include(s => s.MaNccNavigation)
                .Include(s => s.MaLoaiConNavigation)
                .AsQueryable();

            // Lọc theo danh mục
            if (maDanhMuc.HasValue)
            {
                query = query.Where(s => s.FkMaDanhMuc == maDanhMuc.Value);
            }

            // Mặc định chỉ hiển thị sản phẩm có trạng thái "Còn hàng" hoặc "Hết hàng"
            query = query.Where(s => s.TrangThai == "Còn hàng" || s.TrangThai == "Hết hàng");

            // Lọc theo loại sản phẩm (kiểm tra cả MaLoaiCon và MaLoai)
            if (maLoaiCon.HasValue && maLoaiCon.Value > 0)
            {
                int selectedMaLoai = maLoaiCon.Value;
                query = query.Where(s => 
                    (s.MaLoaiCon != null && s.MaLoaiCon == selectedMaLoai) || 
                    (s.MaLoai != null && s.MaLoai == selectedMaLoai)
                );
            }

            // Tìm kiếm theo tên, mã vạch hoặc loại sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                // Lấy danh sách MaLoai của các loại sản phẩm có tên chứa search term
                var maLoais = await _context.LoaiSanPhams
                    .Where(l => l.TenLoai != null && l.TenLoai.Contains(search))
                    .Select(l => l.MaLoai)
                    .ToListAsync();

                query = query.Where(s => 
                    s.Ten.Contains(search) || 
                    (s.MaVach != null && s.MaVach.Contains(search)) ||
                    (s.MaLoaiCon.HasValue && maLoais.Contains(s.MaLoaiCon.Value)) ||
                    (s.MaLoai.HasValue && maLoais.Contains(s.MaLoai.Value))
                );
            }

            // Sắp xếp theo tên
            query = query.OrderBy(s => s.Ten);

            var sanPhams = await query.ToListAsync();

            // Load tất cả loại sản phẩm vào memory để tra cứu
            var allLoaiSanPhams = await _context.LoaiSanPhams.ToListAsync();
            var loaiSanPhamLookup = allLoaiSanPhams.ToDictionary(l => l.MaLoai, l => l);

            // Đảm bảo load loại sản phẩm cho các sản phẩm
            foreach (var sp in sanPhams)
            {
                // Nếu MaLoaiConNavigation null, thử load từ MaLoaiCon hoặc MaLoai
                if (sp.MaLoaiConNavigation == null)
                {
                    // Ưu tiên MaLoaiCon
                    if (sp.MaLoaiCon.HasValue && loaiSanPhamLookup.ContainsKey(sp.MaLoaiCon.Value))
                    {
                        sp.MaLoaiConNavigation = loaiSanPhamLookup[sp.MaLoaiCon.Value];
                    }
                    // Nếu không có MaLoaiCon, thử MaLoai
                    else if (sp.MaLoai.HasValue && loaiSanPhamLookup.ContainsKey(sp.MaLoai.Value))
                    {
                        sp.MaLoaiConNavigation = loaiSanPhamLookup[sp.MaLoai.Value];
                    }
                    // Nếu vẫn không có, thử load từ database
                    else if (sp.MaLoaiCon.HasValue)
                    {
                        try
                        {
                            await _context.Entry(sp)
                                .Reference(s => s.MaLoaiConNavigation)
                                .LoadAsync();
                        }
                        catch { }
                    }
                }
            }
            
            // Truyền dictionary vào ViewData để view có thể sử dụng
            ViewData["LoaiSanPhamDict"] = loaiSanPhamLookup.ToDictionary(l => l.Key, l => l.Value.TenLoai ?? "");

            // Giữ lại giá trị filter để hiển thị lại trong form
            ViewData["Search"] = search;
            ViewData["MaDanhMuc"] = maDanhMuc;
            ViewData["MaLoaiCon"] = maLoaiCon;

            return View(sanPhams);
        }

        // POST: Seller/BanHang/CompletePayment
        [HttpPost]
        public async Task<IActionResult> CompletePayment([FromBody] CompletePaymentRequest request)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var employeeId = HttpContext.Session.GetInt32("EmployeeId");
                if (!employeeId.HasValue)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                // Kiểm tra dữ liệu đầu vào
                if (request == null || request.CartItems == null || !request.CartItems.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng trống" });
                }

                // Lấy thông tin nhân viên đang đăng nhập
                var nhanVien = await _context.NhanViens.FindAsync(employeeId.Value);
                if (nhanVien == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                // Tìm hoặc tạo NguoiDung cho nhân viên này
                int maNv;
                
                // Bước 1: Tìm theo MaNguoiDung = EmployeeId
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(n => n.MaNguoiDung == employeeId.Value);

                // Bước 2: Nếu không tìm thấy, tìm theo tên (chính xác)
                if (nguoiDung == null && !string.IsNullOrEmpty(nhanVien.TenNv))
                {
                    nguoiDung = await _context.NguoiDungs
                        .FirstOrDefaultAsync(n => n.HoTen.Trim() == nhanVien.TenNv.Trim());
                }

                // Bước 3: Nếu vẫn không tìm thấy, tạo mới
                if (nguoiDung == null)
                {
                    // Không tự ý tạo tài khoản với mật khẩu mặc định trong luồng bán hàng
                    // vì sẽ dẫn tới việc "lưu sẵn" mật khẩu cho nhân viên.
                    // Yêu cầu quản trị viên tạo tài khoản đăng nhập riêng cho nhân viên.
                    return Json(new
                    {
                            success = false, 
                        message = "Nhân viên chưa có tài khoản người dùng trong hệ thống. Vui lòng liên hệ quản trị viên để được cấp tài khoản."
                    });
                }

                maNv = nguoiDung.MaNguoiDung;

                // Kiểm tra NguoiDung có tồn tại không
                var nguoiDungCheck = await _context.NguoiDungs.FindAsync(maNv);
                if (nguoiDungCheck == null)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Không tìm thấy người dùng với mã {maNv}. Vui lòng liên hệ quản trị viên." 
                    });
                }

                // Tính toán tổng tiền
                decimal tongTienTruocGiam = 0;
                foreach (var item in request.CartItems)
                {
                    var sanPham = await _context.SanPhams.FindAsync(item.MaSanPham);
                    if (sanPham != null)
                    {
                        tongTienTruocGiam += sanPham.GiaBan * item.SoLuong;
                    }
                }

                decimal tongTienGiamGia = request.Discount;
                decimal tongTienSauGiam = tongTienTruocGiam - tongTienGiamGia;

                // Tạo MaHoaDon đơn giản: HD + timestamp + số ngẫu nhiên
                var random = new Random();
                string maHoaDon = $"HD{DateTime.Now:yyyyMMddHHmmss}{random.Next(1000, 9999)}";
                
                // Kiểm tra xem mã đã tồn tại chưa, nếu có thì tạo lại
                while (await _context.DonHangs.AnyAsync(d => d.MaHoaDon == maHoaDon))
                {
                    maHoaDon = $"HD{DateTime.Now:yyyyMMddHHmmss}{random.Next(1000, 9999)}";
                }

                // Tạo đơn hàng
                var donHang = new DonHang
                {
                    MaHoaDon = maHoaDon,
                    NgayTao = DateTime.Now,
                    FkMaNv = maNv,
                    TongTienTruocGiam = tongTienTruocGiam,
                    TongTienGiamGia = tongTienGiamGia,
                    TongTienSauGiam = tongTienSauGiam,
                    PhuongThucThanhToan = request.PaymentMethod ?? "Tiền mặt"
                };

                try
                {
                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync(); // Lưu để có MaDonHang
                }
                catch (DbUpdateException dbEx)
                {
                    var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                    return Json(new { 
                        success = false, 
                        message = $"Lỗi khi tạo đơn hàng: {innerEx}",
                        details = dbEx.ToString()
                    });
                }

                // Tạo chi tiết đơn hàng
                foreach (var item in request.CartItems)
                {
                    var sanPham = await _context.SanPhams.FindAsync(item.MaSanPham);
                    if (sanPham == null) continue;

                    var chiTietDonHang = new ChiTietDonHang
                    {
                        FkMaDonHang = donHang.MaDonHang,
                        FkMaSanPham = item.MaSanPham,
                        SoLuong = item.SoLuong,
                        GiaBanLucDo = sanPham.GiaBan,
                        GiamGia = 0,
                        Thue = 0,
                        TrangThai = "Đã bán"
                    };

                    _context.ChiTietDonHangs.Add(chiTietDonHang);

                    // Cập nhật số lượng sản phẩm
                    if (sanPham.SoLuong.HasValue)
                    {
                        sanPham.SoLuong -= item.SoLuong;
                        if (sanPham.SoLuong < 0) sanPham.SoLuong = 0;
                        
                        // Cập nhật trạng thái
                        if (sanPham.SoLuong == 0)
                        {
                            sanPham.TrangThai = "Hết hàng";
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    // Log chi tiết lỗi database
                    var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                    var fullError = dbEx.ToString();
                    
                    // Xóa đơn hàng đã tạo nếu có lỗi
                    try
                    {
                        _context.DonHangs.Remove(donHang);
                        await _context.SaveChangesAsync();
                    }
                    catch { }

                    return Json(new { 
                        success = false, 
                        message = $"Lỗi khi lưu chi tiết đơn hàng: {innerEx}",
                        details = fullError
                    });
                }

                return Json(new { 
                    success = true, 
                    message = "Thanh toán thành công!",
                    maDonHang = donHang.MaDonHang
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Lỗi khi lưu đơn hàng: {ex.Message}",
                    details = ex.ToString()
                });
            }
        }
    }

    public class CompletePaymentRequest
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Discount { get; set; } = 0;
    }

    public class CartItem
    {
        public int MaSanPham { get; set; }
        public string Ten { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
    }
}

