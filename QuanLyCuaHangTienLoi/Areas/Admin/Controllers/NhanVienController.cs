using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanVienController : Controller
    {
        private readonly QlchtlContext _context;

        public NhanVienController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Admin/NhanVien
        public async Task<IActionResult> Index()
        {
            var nhanViens = await _context.NhanViens
                .Include(n => n.IdChucvuNavigation)
                .ToListAsync();
            return View(nhanViens);
        }

        // GET: Admin/NhanVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .Include(n => n.IdChucvuNavigation)
                .FirstOrDefaultAsync(m => m.MaNv == id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // GET: Admin/NhanVien/Create
        public IActionResult Create()
        {
            ViewData["IdChucvu"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Chucvus, "IdChucvu", "TenVaiTro");
            return View();
            //ViewData["IdChucvu"] = new SelectList(_context.Chucvus, "IdChucvu", "TenVaiTro");
            //return View();
        }

        // POST: Admin/NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNv,TenNv,MatKhauHash,IdChucvu,Luongcoban,Phucap")] NhanVien nhanVien)
        {
                nhanVien.MatKhauHash = HashPassword("123456");
                
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            
            ViewData["IdChucvu"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Chucvus, "IdChucvu", "TenVaiTro", nhanVien.IdChucvu);
            return View(nhanVien);
        }

        // GET: Admin/NhanVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .Include(n => n.IdChucvuNavigation)
                .FirstOrDefaultAsync(n => n.MaNv == id);
            
            if (nhanVien == null)
            {
                return NotFound();
            }
            
            ViewData["IdChucvu"] = new SelectList(await _context.Chucvus.ToListAsync(), "IdChucvu", "TenVaiTro", nhanVien.IdChucvu);
            return View(nhanVien);
        }

        // POST: Admin/NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            // Lấy nhân viên hiện tại từ database
            var existingNhanVien = await _context.NhanViens.FindAsync(id);
            if (existingNhanVien == null)
            {
                return NotFound();
            }

            try
            {
                // Cập nhật từng trường từ form - không phụ thuộc vào ModelState
                if (Request.Form.ContainsKey("TenNv"))
                {
                    existingNhanVien.TenNv = Request.Form["TenNv"].ToString();
                }

                if (Request.Form.ContainsKey("IdChucvu") && int.TryParse(Request.Form["IdChucvu"], out int idChucvu))
                {
                    existingNhanVien.IdChucvu = idChucvu;
                }

                if (Request.Form.ContainsKey("Luongcoban") && !string.IsNullOrEmpty(Request.Form["Luongcoban"]))
                {
                    if (decimal.TryParse(Request.Form["Luongcoban"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal luongCoban))
                    {
                        existingNhanVien.Luongcoban = luongCoban;
                    }
                }

                if (Request.Form.ContainsKey("Phucap") && !string.IsNullOrEmpty(Request.Form["Phucap"]))
                {
                    if (decimal.TryParse(Request.Form["Phucap"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal phuCap))
                    {
                        existingNhanVien.Phucap = phuCap;
                    }
                }

                // Xử lý DateOnly từ form
                if (Request.Form.ContainsKey("Ngayvaolam") && !string.IsNullOrEmpty(Request.Form["Ngayvaolam"]))
                {
                    if (DateOnly.TryParse(Request.Form["Ngayvaolam"], out DateOnly ngayVaoLam))
                    {
                        existingNhanVien.Ngayvaolam = ngayVaoLam;
                    }
                }
                else
                {
                    // Nếu không có giá trị, giữ nguyên hoặc set null
                    // existingNhanVien.Ngayvaolam = null; // Uncomment nếu muốn xóa khi không có giá trị
                }

                if (Request.Form.ContainsKey("Ngaynghiviec"))
                {
                    if (!string.IsNullOrEmpty(Request.Form["Ngaynghiviec"]))
                    {
                        if (DateOnly.TryParse(Request.Form["Ngaynghiviec"], out DateOnly ngayNghiViec))
                        {
                            existingNhanVien.Ngaynghiviec = ngayNghiViec;
                        }
                    }
                    else
                    {
                        // Nếu trống thì set null
                        existingNhanVien.Ngaynghiviec = null;
                    }
                }

                // Giữ nguyên MatKhauHash - không cập nhật
                // existingNhanVien.MatKhauHash không thay đổi

                _context.Update(existingNhanVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhanVienExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
                // Đảm bảo ViewBag được set lại khi có lỗi
                ViewData["IdChucvu"] = new SelectList(_context.Chucvus.ToList(), "IdChucvu", "TenVaiTro", existingNhanVien.IdChucvu);
                return View(existingNhanVien);
            }
        }

        // GET: Admin/NhanVien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .Include(n => n.IdChucvuNavigation)
                .FirstOrDefaultAsync(m => m.MaNv == id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: Admin/NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                try
                {
                    _context.NhanViens.Remove(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhân viên này vì có dữ liệu liên quan trong hệ thống.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/NhanVien/ResetPassword/5
        public async Task<IActionResult> ResetPassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: Admin/NhanVien/ResetPassword/5
        [HttpPost, ActionName("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirmed(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                nhanVien.MatKhauHash = HashPassword("123456"); // Reset về mật khẩu mặc định
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reset mật khẩu thành công! Mật khẩu mới: 123456";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(int id)
        {
            return _context.NhanViens.Any(e => e.MaNv == id);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(hashedBytes).ToLower();
            }
        }

        #region Thông tin cá nhân
        // GET: Admin/NhanVien/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            // Lấy ID nhân viên từ session
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return RedirectToAction("Index", "DangNhap");
            }

            var nhanVien = await _context.NhanViens
                .Include(n => n.IdChucvuNavigation)
                .FirstOrDefaultAsync(n => n.MaNv == adminId);

            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                return RedirectToAction("Index", "BangDieuKhien");
            }

            return View(nhanVien);
        }

        // POST: Admin/NhanVien/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đổi mật khẩu.";
                return RedirectToAction("Index", "DangNhap");
            }

            var nhanVien = await _context.NhanViens.FindAsync(adminId);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                return RedirectToAction("MyProfile");
            }

            // Kiểm tra mật khẩu cũ
            var oldPasswordHash = HashPassword(model.OldPassword);
            if (nhanVien.MatKhauHash != oldPasswordHash)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng.");
                var nhanVienWithNav = await _context.NhanViens
                    .Include(n => n.IdChucvuNavigation)
                    .FirstOrDefaultAsync(n => n.MaNv == adminId);
                return View("MyProfile", nhanVienWithNav);
            }

            // Kiểm tra mật khẩu mới và xác nhận
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                var nhanVienWithNav = await _context.NhanViens
                    .Include(n => n.IdChucvuNavigation)
                    .FirstOrDefaultAsync(n => n.MaNv == adminId);
                return View("MyProfile", nhanVienWithNav);
            }

            // Kiểm tra độ dài mật khẩu mới
            if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu mới phải có ít nhất 6 ký tự.");
                var nhanVienWithNav = await _context.NhanViens
                    .Include(n => n.IdChucvuNavigation)
                    .FirstOrDefaultAsync(n => n.MaNv == adminId);
                return View("MyProfile", nhanVienWithNav);
            }

            // Cập nhật mật khẩu mới
            nhanVien.MatKhauHash = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
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
}
