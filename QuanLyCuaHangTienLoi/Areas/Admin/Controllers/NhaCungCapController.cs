using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhaCungCapController : Controller
    {

        private readonly QlchtlContext dataContext;

        public NhaCungCapController(QlchtlContext context)
        {
            dataContext = context;
        }
        
        public async Task<IActionResult> Index(string search)
        {
            var query = dataContext.NhaCungCaps.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                // Kiểm tra xem search có phải là số (mã nhà cung cấp) không
                if (int.TryParse(search, out int maNcc))
                {
                    // Tìm theo mã nhà cung cấp HOẶC tên (nếu nhập số vẫn có thể tìm theo tên nếu trùng)
                    query = query.Where(n => n.MaNcc == maNcc || n.TenNcc.Contains(search));
                }
                else
                {
                    // Tìm theo tên nhà cung cấp (không phân biệt hoa thường)
                    query = query.Where(n => n.TenNcc.Contains(search));
                }
            }

            var nhaCungCaps = await query.ToListAsync();
            
            // Giữ lại giá trị search để hiển thị lại trong form
            ViewBag.Search = search;
            
            return View(nhaCungCaps);
        }

        // ==================== CREATE ====================

        // GET: Admin/NhaCungCap/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/NhaCungCap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhaCungCap nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                dataContext.Add(nhaCungCap);
                await dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm nhà cung cấp thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nhaCungCap);
        }

        // ==================== EDIT ====================

        // GET: Admin/NhaCungCap/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhaCungCap = await dataContext.NhaCungCaps.FindAsync(id);
            if (nhaCungCap == null)
            {
                return NotFound();
            }

            return View(nhaCungCap);
        }

        // POST: Admin/NhaCungCap/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhaCungCap nhaCungCap)
        {
            if (id != nhaCungCap.MaNcc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dataContext.Update(nhaCungCap);
                    await dataContext.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật nhà cung cấp thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhaCungCapExists(nhaCungCap.MaNcc))
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
            return View(nhaCungCap);
        }

        // ==================== DELETE ====================

        // GET: Admin/NhaCungCap/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhaCungCap = await dataContext.NhaCungCaps
                .Include(n => n.SanPhams)
                .FirstOrDefaultAsync(m => m.MaNcc == id);

            if (nhaCungCap == null)
            {
                return NotFound();
            }

            // Đếm số sản phẩm đang dùng nhà cung cấp này
            var soSanPham = nhaCungCap.SanPhams?.Count ?? 0;
            ViewBag.SoSanPham = soSanPham;
            ViewBag.CoSanPham = soSanPham > 0;

            return View(nhaCungCap);
        }

        // POST: Admin/NhaCungCap/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhaCungCap = await dataContext.NhaCungCaps.FindAsync(id);
            if (nhaCungCap == null)
            {
                TempData["Error"] = "Không tìm thấy nhà cung cấp cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            var coSanPham = await dataContext.SanPhams.AnyAsync(s => s.MaNcc == id);
            if (coSanPham)
            {
                TempData["Error"] = $"Không thể xóa nhà cung cấp \"{nhaCungCap.TenNcc}\" vì đang có sản phẩm sử dụng!";
            }
            else
            {
                dataContext.NhaCungCaps.Remove(nhaCungCap);
                await dataContext.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa nhà cung cấp \"{nhaCungCap.TenNcc}\" thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NhaCungCapExists(int id)
        {
            return dataContext.NhaCungCaps.Any(e => e.MaNcc == id);
        }
    }
}
