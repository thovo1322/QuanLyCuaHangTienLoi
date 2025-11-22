using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhMucController : Controller
    {
        private readonly QlchtlContext _context;

        public DanhMucController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Admin/DanhMuc
        public async Task<IActionResult> Index()
        {
            var danhMucs = _context.DanhMucs
           .Include("LoaiSanPhams") // Eager loading
           .ToList();

            return View(danhMucs);
        }

        public IActionResult LoaiByDanhMuc(int id)
        {
            var loai = _context.LoaiSanPhams
                .Where(x => x.MaDanhMuc == id)
                .ToList();

            return PartialView("_LoaiSanPhamList", loai);
        }
        // GET: Admin/DanhMuc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMucs
                .Include(l => l.SanPhams)
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id);

            if (danhMuc == null)
            {
                return NotFound();
            }

            return View(danhMuc);
        }

        // GET: Admin/DanhMuc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/DanhMuc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDanhMuc,Ten,MoTa,TrangThai")] DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: Admin/DanhMuc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
            {
                return NotFound();
            }
            return View(danhMuc);
        }

        // POST: Admin/DanhMuc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDanhMuc,Ten,MoTa,TrangThai")] DanhMuc danhMuc)
        {
            if (id != danhMuc.MaDanhMuc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucExists(danhMuc.MaDanhMuc))
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
            return View(danhMuc);
        }

       

        // GET: Admin/DanhMuc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMucs
                .Include(l => l.SanPhams)
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id);

            if (danhMuc == null)
            {
                return NotFound();
            }

            return View(danhMuc);
        }

        // POST: Admin/DanhMuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                // Kiểm tra xem có sản phẩm nào thuộc loại này không
                var hasProducts = await _context.SanPhams.AnyAsync(s => s.FkMaDanhMuc == id);
                if (hasProducts)
                {
                    TempData["ErrorMessage"] = "Không thể xóa loại hàng hóa này vì còn có sản phẩm thuộc loại này!";
                    return RedirectToAction(nameof(Index));
                }

                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.MaDanhMuc == id);
        }
        
    }
}
