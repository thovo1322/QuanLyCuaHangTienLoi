using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangController : Controller
    {
        private readonly QlchtlContext _context;

        public DonHangController(QlchtlContext context)
        {
            _context = context;
        }

        // GET: Admin/DonHang
        public async Task<IActionResult> Index()
        {
            var orders = await _context.DonHangs
                .Include(d => d.FkMaNvNavigation)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/DonHang/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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

        // GET: Admin/DonHang/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }

            ViewData["FkMaNv"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.NhanViens, "MaNv", "TenNv", donHang.FkMaNv);
            return View(donHang);
        }

        // POST: Admin/DonHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDonHang,MaHoaDon,FkMaNv,NgayTao,TongTienTruocGiam,TongTienGiamGia,TongTienSauGiam,PhuongThucThanhToan")] DonHang donHang)
        {
            if (id != donHang.MaDonHang)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(donHang.MaDonHang))
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
            ViewData["FkMaNv"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.NhanViens, "MaNv", "TenNv", donHang.FkMaNv);
            return View(donHang);
        }

        // GET: Admin/DonHang/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.FkMaNvNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: Admin/DonHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                _context.DonHangs.Remove(donHang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.MaDonHang == id);
        }
    }
}
