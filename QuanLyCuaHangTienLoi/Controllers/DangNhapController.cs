using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyCuaHangTienLoi.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly QlchtlContext _context;

        public DangNhapController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: DangNhap - Trang chủ là trang đăng nhập
        public IActionResult Index()
        {
            // Nếu đã đăng nhập admin thì redirect về admin dashboard
            if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
            {
                return RedirectToAction("Index", "BangDieuKhien", new { area = "Admin" });
            }
            
            // Nếu đã đăng nhập nhân viên thì redirect về nhân viên dashboard
            if (HttpContext.Session.GetString("EmployeeLoggedIn") == "true")
            {
                return RedirectToAction("Index", "Home", new { area = "Seller" });
            }
            
            return View();
        }

        // POST: DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DangNhapViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm nhân viên theo tên đăng nhập
                var nhanVien = await _context.NhanViens
                    .Include(n => n.IdChucvuNavigation)
                    .FirstOrDefaultAsync(n => n.TenNv == model.Username);

                if (nhanVien != null)
                {
                    // Kiểm tra mật khẩu
                    var hashedPassword = HashPassword(model.Password);
                    
                    // Debug: Log thông tin để kiểm tra
                    var chucVuInfo = nhanVien.IdChucvuNavigation?.TenVaiTro ?? "NULL";
                    var idChucVu = nhanVien.IdChucvu;
                    
                    if (nhanVien.MatKhauHash == hashedPassword)
                    {
                        // Kiểm tra chức vụ để redirect đúng
                        var chucVu = nhanVien.IdChucvuNavigation?.TenVaiTro?.Trim() ?? "";
                        var idChucvu = nhanVien.IdChucvu;
                        
                        // Nếu là Quản lý cửa hàng (IdChucvu = 1) → redirect về Admin area
                        if (idChucvu == 1 || chucVu.Contains("Quản lý") || chucVu.ToLower().Contains("admin"))
                        {
                            // Lưu thông tin đăng nhập admin vào session
                            HttpContext.Session.SetString("AdminLoggedIn", "true");
                            HttpContext.Session.SetString("AdminName", nhanVien.TenNv);
                            HttpContext.Session.SetInt32("AdminId", nhanVien.MaNv);

                            return RedirectToAction("Index", "BangDieuKhien", new { area = "Admin" });
                        }
                        // Nếu là Nhân viên cửa hàng (IdChucvu = 2) → redirect về Seller area
                        else if (idChucvu == 2 || chucVu.Contains("Nhân viên"))
                        {
                            // Lưu thông tin đăng nhập nhân viên vào session
                            HttpContext.Session.SetString("EmployeeLoggedIn", "true");
                            HttpContext.Session.SetString("EmployeeName", nhanVien.TenNv);
                            HttpContext.Session.SetInt32("EmployeeId", nhanVien.MaNv);

                            return RedirectToAction("Index", "Home", new { area = "Seller" });
                        }
                        else
                        {
                            // Debug: Hiển thị thông tin chức vụ để kiểm tra
                            ModelState.AddModelError("", $"Chức vụ không hợp lệ. ID: {idChucvu}, Tên: '{chucVu}'. Vui lòng liên hệ quản trị viên.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        // GET: DangXuat
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
            return RedirectToAction("Index");
        }

        // GET: QuenMatKhau
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        // POST: QuenMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(QuenMatKhauViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm nhân viên theo tên đăng nhập
                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(n => n.TenNv == model.Username);

                if (nhanVien != null)
                {
                    // Reset mật khẩu về mật khẩu mặc định
                    string defaultPassword = "123456";
                    nhanVien.MatKhauHash = HashPassword(defaultPassword);
                    
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Mật khẩu đã được reset về mặc định: {defaultPassword}. Vui lòng đăng nhập và đổi mật khẩu.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập không tồn tại trong hệ thống.");
                }
            }

            return View(model);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(hashedBytes).ToLower();
            }
        }
    }

    public class DangNhapViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; }
    }

    public class QuenMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; } = string.Empty;
    }
}

