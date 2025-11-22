using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

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

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }
            ViewData["IdChucvu"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Chucvus, "IdChucvu", "TenVaiTro", nhanVien.IdChucvu);
            return View(nhanVien);
        }

        // POST: Admin/NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNv,TenNv,MatKhauHash,IdChucvu,Luongcoban,Phucap")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNv)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdChucvu"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Chucvus, "IdChucvu", "TenVaiTro", nhanVien.IdChucvu);
            return View(nhanVien);
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
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
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
    }
}
