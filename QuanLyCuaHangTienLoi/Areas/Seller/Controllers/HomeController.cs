using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangTienLoi.Areas.Seller.Controllers
{
    [Area("Seller")]
    public class HomeController : Controller
    {
        private readonly QlchtlContext _context;

        public HomeController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Seller/Home
        public async Task<IActionResult> Index(DateTime? selectedDate)
        {
            // Kiểm tra đăng nhập nhân viên
            if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            int soNgayLamViec = 0;

            if (employeeId.HasValue)
            {
                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(n => n.MaNv == employeeId.Value);

                if (nhanVien != null && nhanVien.Ngayvaolam.HasValue)
                {
                    // Tính số ngày làm việc từ ngày vào làm đến hiện tại
                    var ngayVaoLam = nhanVien.Ngayvaolam.Value;
                    var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                    soNgayLamViec = (ngayHienTai.ToDateTime(TimeOnly.MinValue) - ngayVaoLam.ToDateTime(TimeOnly.MinValue)).Days;
                    if (soNgayLamViec < 0) soNgayLamViec = 0;
                }
            }

            // Mặc định là ngày hôm nay nếu không chọn
            if (!selectedDate.HasValue)
            {
                selectedDate = DateTime.Today;
            }

            // Không cho phép chọn ngày tương lai
            if (selectedDate.Value.Date > DateTime.Today.Date)
            {
                selectedDate = DateTime.Today;
            }

            // Lấy danh sách đơn hàng theo ngày được chọn
            var startDate = selectedDate.Value.Date;
            var endDate = startDate.AddDays(1);

            var ordersByDate = await _context.DonHangs
                .Include(d => d.FkMaNvNavigation)
                .Where(d => d.NgayTao.HasValue && 
                           d.NgayTao.Value.Date >= startDate && 
                           d.NgayTao.Value.Date < endDate)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            var totalAmount = ordersByDate.Sum(o => o.TongTienSauGiam);

            var viewModel = new SellerHomeViewModel
            {
                RecentOrders = ordersByDate,
                SelectedDate = selectedDate.Value,
                TotalAmount = totalAmount
            };

            ViewData["Title"] = "Trang chủ nhân viên";
            ViewData["EmployeeName"] = HttpContext.Session.GetString("EmployeeName");
            ViewData["SoNgayLamViec"] = soNgayLamViec;
            return View(viewModel);
        }

        // GET: Seller/Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.FkMaNvNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.FkMaSanPhamNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // GET: Seller/Home/DangXuat
        public IActionResult DangXuat()
        {
            // Xóa tất cả session keys
            HttpContext.Session.Remove("AdminLoggedIn");
            HttpContext.Session.Remove("AdminName");
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("EmployeeLoggedIn");
            HttpContext.Session.Remove("EmployeeName");
            HttpContext.Session.Remove("EmployeeId");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "DangNhap", new { area = "" });
        }

        #region Thông tin cá nhân
        // GET: Seller/Home/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            // Lấy ID nhân viên từ session
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var nhanVien = await _context.NhanViens
                .Include(n => n.IdChucvuNavigation)
                .FirstOrDefaultAsync(n => n.MaNv == employeeId);

            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                return RedirectToAction("Index");
            }

            // Lấy thông tin liên hệ từ bảng NguoiDung (nếu có)
            NguoiDung? nguoiDung = null;

            try
            {
                nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(n =>
                        n.MaNguoiDung == employeeId.Value ||
                        (nhanVien.TenNv != null && n.HoTen.Trim() == nhanVien.TenNv.Trim()));
            }
            catch
            {
                // Bỏ qua lỗi truy vấn, chỉ đơn giản là không có thông tin liên hệ
            }

            var viewModel = new SellerProfileViewModel
            {
                MaNv = nhanVien.MaNv,
                TenNv = nhanVien.TenNv,
                ChucVu = nhanVien.IdChucvuNavigation?.TenVaiTro,
                Luongcoban = nhanVien.Luongcoban,
                Phucap = nhanVien.Phucap,
                Ngayvaolam = nhanVien.Ngayvaolam,
                Ngaynghiviec = nhanVien.Ngaynghiviec,
                Email = nguoiDung?.Email,
                SoDienThoai = nguoiDung?.SoDienThoai,
                DiaChi = nguoiDung?.DiaChi
            };

            return View(viewModel);
        }

        // POST: Seller/Home/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đổi mật khẩu.";
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var nhanVien = await _context.NhanViens.FindAsync(employeeId);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                return RedirectToAction("MyProfile");
            }

            // Kiểm tra mật khẩu cũ
            var oldPasswordHash = HashPassword(model.OldPassword);
            if (nhanVien.MatKhauHash != oldPasswordHash)
            {
                TempData["ErrorMessage"] = "Mật khẩu cũ không đúng.";
                return RedirectToAction("MyProfile");
            }

            // Kiểm tra mật khẩu mới và xác nhận
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return RedirectToAction("MyProfile");
            }

            // Kiểm tra độ dài mật khẩu mới
            if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return RedirectToAction("MyProfile");
            }

            // Cập nhật mật khẩu mới
            nhanVien.MatKhauHash = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("MyProfile");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(hashedBytes).ToLower();
            }
        }

        // Cập nhật thông tin liên hệ: địa chỉ, số điện thoại, email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactInfo(SellerProfileViewModel model)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("EmployeeLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để cập nhật thông tin.";
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(n => n.MaNv == employeeId.Value);

            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                return RedirectToAction("MyProfile");
            }

            // Tìm người dùng tương ứng (nếu chưa có thì tạo mới)
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(n =>
                    n.MaNguoiDung == employeeId.Value ||
                    (nhanVien.TenNv != null && n.HoTen.Trim() == nhanVien.TenNv.Trim()));

            if (nguoiDung == null)
            {
                // Lấy quyền hạn mặc định đầu tiên
                var quyenHan = await _context.Quyenhans
                    .OrderBy(q => q.IdQuyenhan)
                    .FirstOrDefaultAsync();

                if (quyenHan == null)
                {
                    TempData["ErrorMessage"] = "Hệ thống chưa cấu hình quyền hạn. Vui lòng liên hệ quản trị viên.";
                    return RedirectToAction("MyProfile");
                }

                // Tạo tên đăng nhập unique
                string tenDangNhap = $"nv{employeeId.Value}";
                int counter = 1;
                while (await _context.NguoiDungs.AnyAsync(n => n.TenDangNhap == tenDangNhap))
                {
                    tenDangNhap = $"nv{employeeId.Value}_{counter}";
                    counter++;
                    if (counter > 1000)
                    {
                        TempData["ErrorMessage"] = "Không thể tạo tên đăng nhập. Vui lòng liên hệ quản trị viên.";
                        return RedirectToAction("MyProfile");
                    }
                }

                nguoiDung = new NguoiDung
                {
                    HoTen = nhanVien.TenNv ?? "Nhân viên",
                    TenDangNhap = tenDangNhap,
                    MatKhau = "123456",
                    TrangThai = true,
                    FkIdQuyenHan = quyenHan.IdQuyenhan,
                    NgayTao = DateTime.Now
                };

                _context.NguoiDungs.Add(nguoiDung);
            }

            // Kiểm tra trùng email (nếu có nhập)
            var newEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            if (!string.IsNullOrEmpty(newEmail))
            {
                var emailExists = await _context.NguoiDungs
                    .AnyAsync(n => n.Email == newEmail && n.MaNguoiDung != nguoiDung.MaNguoiDung);

                if (emailExists)
                {
                    TempData["ErrorMessage"] = "Email này đã được sử dụng bởi người dùng khác.";
                    return RedirectToAction("MyProfile");
                }
            }

            nguoiDung.Email = newEmail;
            nguoiDung.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                ? null
                : model.SoDienThoai.Trim();
            nguoiDung.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi)
                ? null
                : model.DiaChi.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thông tin liên hệ thành công!";
            return RedirectToAction("MyProfile");
        }
        #endregion
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class SellerHomeViewModel
    {
        public List<DonHang> RecentOrders { get; set; } = new List<DonHang>();
        public List<HoaDon> RecentHoaDons { get; set; } = new List<HoaDon>();
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public decimal TotalAmount { get; set; } = 0;
    }

    public class SellerProfileViewModel
    {
        public int MaNv { get; set; }
        public string? TenNv { get; set; }
        public string? ChucVu { get; set; }
        public decimal? Luongcoban { get; set; }
        public decimal? Phucap { get; set; }
        public DateOnly? Ngayvaolam { get; set; }
        public DateOnly? Ngaynghiviec { get; set; }

        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
    }
}

