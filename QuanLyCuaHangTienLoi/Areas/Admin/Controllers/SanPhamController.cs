using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangTienLoi.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class SanPhamController : Controller
    {
        private readonly QlchtlContext dataContext;
        public SanPhamController(QlchtlContext context)
        {
            dataContext = context;
        }
        public async Task<IActionResult> Index(string search)
        {
            var query = dataContext.SanPhams
                .Include(s => s.FkMaDanhMucNavigation)
                
                .Include(s => s.MaNccNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Ten.Contains(search) || s.MaVach.Contains(search));
            }

            var sanPhams = await query.ToListAsync();
            return View(sanPhams);
        }

        // GET: SanPhams/Create
        public IActionResult Create()
        {
            //ViewBag.DanhMucs = new List<DanhMuc>(dataContext.DanhMucs);
            //ViewBag.NhaCungCaps = new List<NhaCungCap>(dataContext.NhaCungCaps);
            ViewBag.DanhMucs = new SelectList(
                dataContext.DanhMucs.ToList(),
                "MaDanhMuc",      // Tên thuộc tính dùng làm Value (giá trị lưu vào DB)
                "Ten"     // Tên thuộc tính dùng làm Text (giá trị hiển thị)
               
            );

            // Tạo SelectList cho Nhà Cung Cấp
            ViewBag.NhaCungCaps = new SelectList(
                dataContext.NhaCungCaps.ToList(),
                "MaNcc",          // Tên thuộc tính dùng làm Value
                "TenNcc"        // Tên thuộc tính dùng làm Text
                
            );
            // Trong SanPhamController.cs, action Create():



            return View(new SanPham());
            
        }

        // POST: SanPhams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sp)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    sp.NgayTao = DateTime.Now;
                    sp.TrangThai = "Còn hàng";
                    sp.FkMaNguoiTao = 3;
                    dataContext.Add(sp);
                    await dataContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                catch (Exception e)
                {
                    var eror = new ErrorViewModel
                    {
                        RequestId = "Lỗi khi tạo sản phẩm ",
                        Message = e.Message 
                    };

                    ModelState.AddModelError(string.Empty, "Lỗi khi tạo sản phẩm: " + e.Message);
                    return View("Error", eror);
                }
            }
            //ViewBag.DanhMucs = new SelectList(
            //    dataContext.DanhMucs.ToList(),
            //    "MaDanhMuc",      // Tên thuộc tính dùng làm Value (giá trị lưu vào DB)
            //    "TenDanhMuc"      // Tên thuộc tính dùng làm Text (giá trị hiển thị)
            //);

            //// Tạo SelectList cho Nhà Cung Cấp
            //ViewBag.NhaCungCaps = new SelectList(
            //    dataContext.NhaCungCaps.ToList(),
            //    "MaNcc",          // Tên thuộc tính dùng làm Value
            //    "TenNcc"          // Tên thuộc tính dùng làm Text
            //);
            //ViewBag.DanhMucs = dataContext.DanhMucs.ToList();

            //ViewBag.NhaCungCaps = dataContext.NhaCungCaps.ToList();
            //ViewBag.DanhMucs = new List<DanhMuc>(dataContext.DanhMucs);
            //ViewBag.NhaCungCaps = new List<NhaCungCap>(dataContext.NhaCungCaps);
            return View(sp);
        }

        // GET: SanPhams/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sp = await dataContext.SanPhams.FindAsync(id);
            if (sp == null)
            {
                return NotFound();
               
            }
            ViewBag.DanhMucs = new SelectList(
                dataContext.DanhMucs.ToList(),
                "MaDanhMuc",      // Tên thuộc tính dùng làm Value (giá trị lưu vào DB)
                "Ten"     // Tên thuộc tính dùng làm Text (giá trị hiển thị)

            );

            // Tạo SelectList cho Nhà Cung Cấp
            ViewBag.NhaCungCaps = new SelectList(
                dataContext.NhaCungCaps.ToList(),
                "MaNcc",          // Tên thuộc tính dùng làm Value
                "TenNcc"        // Tên thuộc tính dùng làm Text

            );

            return View(sp);
        }

        // POST: SanPhams/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSP(SanPham sp)
        {
            //       if (id != sp.MaSanPham)
            //       {
            //           return NotFound();
            //       }

            //       if (ModelState.IsValid)
            //       {
            //           try
            //           {
            //               dataContext.Update(sp);
            //               await dataContext.SaveChangesAsync();
            //           }
            //           catch (DbUpdateConcurrencyException)
            //           {
            //               if (!dataContext.SanPhams.Any(e => e.MaSanPham == id))
            //                   return NotFound();
            //               else
            //                   throw;
            //           }
            //           return RedirectToAction(nameof(Index));
            //       }
            //       ViewBag.DanhMucs = new SelectList(
            //    dataContext.DanhMucs.ToList(),
            //    "MaDanhMuc",
            //    "Ten"
            //);
            //       ViewBag.NhaCungCaps = new SelectList(
            //           dataContext.NhaCungCaps.ToList(),
            //           "MaNcc",          // Tên thuộc tính dùng làm Value
            //           "TenNcc"
            //       // Tên thuộc tính dùng làm Text

            //       );
            //       return View(sp);
            //   }
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var existingSp = dataContext.SanPhams.Find(sp.MaSanPham);

                        if (existingSp == null) {
                            return NotFound();
                        }
                        existingSp.FkMaDanhMuc = sp.FkMaDanhMuc;
                        existingSp.MaNcc = sp.MaNcc;
                        existingSp.Ten = sp.Ten;
                        existingSp.GiaBan = sp.GiaBan;
                        existingSp.GiaVon = sp.GiaVon;
                        existingSp.SoLuong = sp.SoLuong;
                        // BỎ QUA ModelState, ép EF hiểu là đang sửa
                        if (existingSp.SoLuong > 0)
                        {
                            existingSp.TrangThai = "Còn hàng";
                        }
                        else {
                            existingSp.TrangThai = "Hết hàng";
                        }
                        
                        dataContext.SanPhams.Update(existingSp);
                        await dataContext.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Không lưu được: " + ex.Message);
                    }
                }

                // Set lại dropdown
                ViewBag.DanhMucs = new SelectList(dataContext.DanhMucs.ToList(), "MaDanhMuc", "Ten");
                ViewBag.NhaCungCaps = new SelectList(dataContext.NhaCungCaps.ToList(), "MaNcc", "TenNcc");


                return View(sp);
            }
        }
        // GET: SanPhams/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var sp = await dataContext.SanPhams
                .Include(s => s.FkMaDanhMucNavigation)
                .FirstOrDefaultAsync(m => m.MaSanPham == id);

            if (sp == null)
                return NotFound();

            return View(sp);
        }

        // POST: SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sp = await dataContext.SanPhams.FindAsync(id);
            if (sp != null)
            {
                dataContext.SanPhams.Remove(sp);
                await dataContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult ByLoai(int id)
        {
            var sp = dataContext.SanPhams
                .Where(x => x.MaLoai == id)
                .ToList();

            return View(sp);
        }

    }
}
//        public IActionResult Index()

//        {
//            return View();
//        }
//    }
//}
