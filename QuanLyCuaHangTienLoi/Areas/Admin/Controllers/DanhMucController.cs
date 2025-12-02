using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.DanhMucs
                .Include("LoaiSanPhams")
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Ten.Contains(search));
            }

            var danhMucs = await query.ToListAsync();
            
            // Giữ lại giá trị search để hiển thị lại trong form
            ViewBag.Search = search;
            
            return View(danhMucs);
        }

        // API: Lấy danh sách gợi ý cho autocomplete
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<string>());
            }

            var suggestions = await _context.DanhMucs
                .Where(d => d.Ten.Contains(term))
                .Select(d => d.Ten)
                .Take(10)
                .ToListAsync();

            return Json(suggestions);
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
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            // Lấy AdminId từ session
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm danh mục!";
                return RedirectToAction("Index", "DangNhap", new { area = "" });
            }

            // Validate các trường bắt buộc
            // MaDanhMuc là identity column, không cần validate hoặc set giá trị
            if (string.IsNullOrWhiteSpace(danhMuc.Ten))
            {
                ModelState.AddModelError("Ten", "Tên danh mục là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(danhMuc.TrangThai))
            {
                ModelState.AddModelError("TrangThai", "Trạng thái là bắt buộc");
            }

            // FkMaNguoiTao phải tham chiếu đến MaNguoiDung trong bảng NguoiDung
            // Tìm NguoiDung đầu tiên có sẵn hoặc tạo mới nếu cần
            var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync();
            
            if (nguoiDung == null)
            {
                // Không tự động tạo người dùng với mật khẩu mặc định để tránh lưu sẵn mật khẩu.
                ModelState.AddModelError(string.Empty,
                    "Không tìm thấy người dùng hợp lệ để gán cho danh mục. Vui lòng tạo/cấu hình tài khoản người dùng (NguoiDung) trước.");
                return View(danhMuc);
            }

            danhMuc.FkMaNguoiTao = nguoiDung.MaNguoiDung;

            // Xóa lỗi validation cho navigation properties (không cần thiết khi tạo mới)
            ModelState.Remove("FkMaNguoiTaoNavigation");
            ModelState.Remove("LoaiSanPhams");
            ModelState.Remove("SanPhams");

            if (ModelState.IsValid)
            {
                try
                {
                    // Tạo entity mới với các giá trị đã validate
                    // MaDanhMuc là identity column, sẽ được database tự động generate
                    var newDanhMuc = new DanhMuc
                    {
                        // Không set MaDanhMuc - để database tự động tạo
                        Ten = danhMuc.Ten.Trim(),
                        MoTa = string.IsNullOrWhiteSpace(danhMuc.MoTa) ? null : danhMuc.MoTa.Trim(),
                        TrangThai = danhMuc.TrangThai.Trim(),
                        FkMaNguoiTao = danhMuc.FkMaNguoiTao
                    };

                    _context.DanhMucs.Add(newDanhMuc);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Thêm danh mục \"{newDanhMuc.Ten}\" (Mã: {newDanhMuc.MaDanhMuc}) thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var innerEx = ex.InnerException;
                    var errorMessage = innerEx?.Message ?? ex.Message;
                    
                    // Hiển thị lỗi chi tiết
                    var fullError = innerEx != null ? $"{ex.Message} | Inner: {errorMessage}" : errorMessage;
                    
                    if (errorMessage.Contains("PRIMARY KEY") || errorMessage.Contains("duplicate key") || 
                        errorMessage.Contains("UNIQUE") || errorMessage.Contains("Violation of PRIMARY KEY"))
                    {
                        ModelState.AddModelError("MaDanhMuc", "Mã danh mục đã tồn tại. Vui lòng chọn mã khác.");
                    }
                    else if (errorMessage.Contains("FOREIGN KEY") || errorMessage.Contains("FkMaNguoiTao") ||
                             errorMessage.Contains("FK_MaNguoiTao") || errorMessage.Contains("reference constraint"))
                    {
                        ModelState.AddModelError("", "Lỗi: Không tìm thấy thông tin người tạo trong hệ thống. Vui lòng liên hệ quản trị viên.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Lỗi khi thêm danh mục: {fullError}");
                    }
                }
                catch (Exception ex)
                {
                    var innerEx = ex.InnerException;
                    var errorMessage = innerEx?.Message ?? ex.Message;
                    var fullError = innerEx != null ? $"{ex.Message} | Inner: {errorMessage}" : errorMessage;
                    ModelState.AddModelError("", $"Lỗi khi thêm danh mục: {fullError}");
                }
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
                    TempData["Success"] = "Cập nhật danh mục thành công!";
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
                .Include(l => l.LoaiSanPhams)
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id);

            if (danhMuc == null)
            {
                return NotFound();
            }

            // Đếm số sản phẩm và loại sản phẩm
            var soSanPham = danhMuc.SanPhams?.Count ?? 0;
            var soLoaiSanPham = danhMuc.LoaiSanPhams?.Count ?? 0;
            
            ViewBag.SoSanPham = soSanPham;
            ViewBag.SoLoaiSanPham = soLoaiSanPham;
            ViewBag.CoRangBuoc = (soSanPham > 0 || soLoaiSanPham > 0);

            return View(danhMuc);
        }

        // POST: Admin/DanhMuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem có sản phẩm nào thuộc danh mục này không
            var hasProducts = await _context.SanPhams.AnyAsync(s => s.FkMaDanhMuc == id);
            // Kiểm tra xem có loại sản phẩm nào thuộc danh mục này không
            var hasLoaiSanPham = await _context.LoaiSanPhams.AnyAsync(l => l.MaDanhMuc == id);
            
            if (hasProducts || hasLoaiSanPham)
            {
                TempData["Error"] = $"Không thể xóa danh mục \"{danhMuc.Ten}\" vì đang có sản phẩm hoặc loại sản phẩm sử dụng!";
            }
            else
            {
                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa danh mục \"{danhMuc.Ten}\" thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.MaDanhMuc == id);
        }
        
    }
}
