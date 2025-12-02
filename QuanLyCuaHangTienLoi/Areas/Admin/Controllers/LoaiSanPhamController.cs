using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Create()
        {
            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "Ten");
            return View();
        }

        // POST: Admin/LoaiSanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiSanPham loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiSanPham);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm loại sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi thì load lại dropdown
            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "Ten", loaiSanPham.MaDanhMuc);
            return View(loaiSanPham);
        }

        // ==================== EDIT ====================

        // GET: Admin/LoaiSanPham/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiSanPham = await _context.LoaiSanPhams.FindAsync(id);
            if (loaiSanPham == null)
            {
                return NotFound();
            }

            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "Ten", loaiSanPham.MaDanhMuc);
            return View(loaiSanPham);
        }

        // POST: Admin/LoaiSanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LoaiSanPham loaiSanPham)
        {
            if (id != loaiSanPham.MaLoai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiSanPham);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật loại sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiSanPhamExists(loaiSanPham.MaLoai))
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

            // Nếu lỗi thì load lại dropdown
            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "Ten", loaiSanPham.MaDanhMuc);
            return View(loaiSanPham);
        }

        private bool LoaiSanPhamExists(int id)
        {
            return _context.LoaiSanPhams.Any(e => e.MaLoai == id);
        }

        // ==================== DELETE ====================

        // GET: Admin/LoaiSanPham/Delete/5
        // GET: Admin/LoaiSanPham/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loai = await _context.LoaiSanPhams
                .Include(l => l.MaDanhMucNavigation)
                .FirstOrDefaultAsync(m => m.MaLoai == id);

            if (loai == null) return NotFound();

            // Đếm số sản phẩm đang dùng loại này
            var soSanPham = await _context.SanPhams.CountAsync(s => s.MaLoai == id);
            ViewBag.SoSanPham = soSanPham;
            ViewBag.CoSanPham = soSanPham > 0;

            return View(loai);
        }

        // POST: Xóa xác nhận
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loai = await _context.LoaiSanPhams.FindAsync(id);
            if (loai == null)
            {
                TempData["Error"] = "Không tìm thấy loại sản phẩm cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            var coSanPham = await _context.SanPhams.AnyAsync(s => s.MaLoai == id);
            if (coSanPham)
            {
                TempData["Error"] = $"Không thể xóa loại \"{loai.TenLoai}\" vì đang có sản phẩm sử dụng!";
            }
            else
            {
                _context.LoaiSanPhams.Remove(loai);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa loại \"{loai.TenLoai}\" thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

    

