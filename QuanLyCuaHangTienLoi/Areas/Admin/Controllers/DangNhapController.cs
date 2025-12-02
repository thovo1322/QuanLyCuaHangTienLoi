using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DangNhapController : Controller
    {
        private readonly QlchtlContext _context;

        public DangNhapController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Admin/DangNhap
        public IActionResult Index()
        {
            // Nếu đã đăng nhập thì redirect về dashboard
            if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
            {
                return RedirectToAction("Index", "BangDieuKhien");
            }
            return View();
        }

        // POST: Admin/DangNhap
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
                    // Kiểm tra mật khẩu (giả sử mật khẩu được hash)
                    var hashedPassword = HashPassword(model.Password);
                    
                    if (nhanVien.MatKhauHash == hashedPassword)
                    {
                        // Kiểm tra quyền admin (chức vụ "Quản lý cửa hàng")
                        if (nhanVien.IdChucvuNavigation?.TenVaiTro?.Contains("Quản lý") == true || 
                            nhanVien.IdChucvuNavigation?.TenVaiTro?.ToLower().Contains("admin") == true)
                        {
                            // Lưu thông tin đăng nhập vào session
                            HttpContext.Session.SetString("AdminLoggedIn", "true");
                            HttpContext.Session.SetString("AdminName", nhanVien.TenNv);
                            HttpContext.Session.SetInt32("AdminId", nhanVien.MaNv);

                            return RedirectToAction("Index", "BangDieuKhien");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Bạn không có quyền truy cập trang admin.");
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

        // GET: Admin/DangXuat
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

        // GET: Admin/DangNhap/QuenMatKhau
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        // POST: Admin/DangNhap/QuenMatKhau
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
                    string defaultPassword = "123456"; // Mật khẩu mặc định
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
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class QuenMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; } = string.Empty;
    }
}
