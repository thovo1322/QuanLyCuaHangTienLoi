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
            
            // Tự động đồng bộ trạng thái với số lượng
            bool hasChanges = false;
            foreach (var sp in sanPhams)
            {
                // Nếu trạng thái không hợp lệ, tự động cập nhật dựa trên số lượng
                if (sp.TrangThai != "Còn hàng" && sp.TrangThai != "Hết hàng")
                {
                    if (sp.SoLuong.HasValue && sp.SoLuong.Value > 0)
                    {
                        sp.TrangThai = "Còn hàng";
                    }
                    else
                    {
                        sp.TrangThai = "Hết hàng";
                    }
                    hasChanges = true;
                }
                // Nếu trạng thái là "Còn hàng" nhưng số lượng = 0, cập nhật thành "Hết hàng"
                else if (sp.TrangThai == "Còn hàng" && (!sp.SoLuong.HasValue || sp.SoLuong.Value <= 0))
                {
                    sp.TrangThai = "Hết hàng";
                    hasChanges = true;
                }
                // Nếu trạng thái là "Hết hàng" nhưng số lượng > 0, cập nhật thành "Còn hàng"
                else if (sp.TrangThai == "Hết hàng" && sp.SoLuong.HasValue && sp.SoLuong.Value > 0)
                {
                    sp.TrangThai = "Còn hàng";
                    hasChanges = true;
                }
            }
            
            // Lưu thay đổi nếu có
            if (hasChanges)
            {
                await dataContext.SaveChangesAsync();
            }
            
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
            // Set lại dropdown để hiển thị khi có lỗi
            ViewBag.DanhMucs = new SelectList(dataContext.DanhMucs.ToList(), "MaDanhMuc", "Ten");
            ViewBag.NhaCungCaps = new SelectList(dataContext.NhaCungCaps.ToList(), "MaNcc", "TenNcc");
            
            try
            {
                // Parse giá từ Request.Form (bỏ dấu phẩy và parse)
                string giaBanStr = "";
                string giaVonStr = "";
                
                if (Request.Form.ContainsKey("GiaBan"))
                {
                    giaBanStr = Request.Form["GiaBan"].ToString().Replace(",", "").Replace(".", "").Trim();
                }
                
                if (Request.Form.ContainsKey("GiaVon"))
                {
                    giaVonStr = Request.Form["GiaVon"].ToString().Replace(",", "").Replace(".", "").Trim();
                }
                
                // Parse giá bán từ form
                if (!string.IsNullOrEmpty(giaBanStr))
                {
                    if (decimal.TryParse(giaBanStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal giaBanParsed))
                    {
                        sp.GiaBan = giaBanParsed;
                    }
                    else
                    {
                        ModelState.AddModelError("GiaBan", "Giá bán không hợp lệ");
                    }
                }
                else
                {
                    ModelState.AddModelError("GiaBan", "Vui lòng nhập giá bán");
                }
                
                // Parse giá vốn từ form
                if (!string.IsNullOrEmpty(giaVonStr))
                {
                    if (decimal.TryParse(giaVonStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal giaVonParsed))
                    {
                        sp.GiaVon = giaVonParsed;
                    }
                    else
                    {
                        ModelState.AddModelError("GiaVon", "Giá vốn không hợp lệ");
                    }
                }
                else
                {
                    // Giá vốn có thể để trống, mặc định là 0
                    sp.GiaVon = 0;
                }

                // Validate các trường bắt buộc
                if (string.IsNullOrWhiteSpace(sp.Ten))
                {
                    ModelState.AddModelError("Ten", "Tên sản phẩm là bắt buộc");
                }
                
                if (sp.GiaBan <= 0)
                {
                    ModelState.AddModelError("GiaBan", "Giá bán phải lớn hơn 0");
                }
                
                if (sp.FkMaDanhMuc <= 0)
                {
                    ModelState.AddModelError("FkMaDanhMuc", "Vui lòng chọn danh mục");
                }

                // Kiểm tra mã vạch trùng (nếu có)
                if (!string.IsNullOrWhiteSpace(sp.MaVach))
                {
                    var existingProduct = await dataContext.SanPhams
                        .FirstOrDefaultAsync(s => s.MaVach == sp.MaVach);
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("MaVach", "Mã vạch này đã tồn tại. Vui lòng sử dụng mã vạch khác.");
                    }
                }

                // FkMaNguoiTao phải tham chiếu đến MaNguoiDung trong bảng NguoiDung
                // Lấy NguoiDung đầu tiên có sẵn (giống như DanhMucController)
                var nguoiDung = await dataContext.NguoiDungs.FirstOrDefaultAsync();
                
                if (nguoiDung == null)
                {
                    // Không tự động tạo tài khoản người dùng với mật khẩu mặc định,
                    // tránh việc lưu sẵn mật khẩu. Yêu cầu cấu hình trước bảng NguoiDung.
                    ModelState.AddModelError(string.Empty,
                        "Không tìm thấy người dùng hợp lệ để gán cho sản phẩm. Vui lòng tạo/cấu hình tài khoản người dùng (NguoiDung) trước.");
                        return View(sp);
                }

                // Xóa lỗi validation cho các navigation properties (không cần thiết khi tạo mới)
                ModelState.Remove("FkMaDanhMucNavigation");
                ModelState.Remove("FkMaNguoiTaoNavigation");
                ModelState.Remove("MaNccNavigation");

                if (ModelState.IsValid)
                {
                    sp.NgayTao = DateTime.Now;
                    // Tự động xác định trạng thái dựa trên số lượng
                    if (sp.SoLuong.HasValue && sp.SoLuong.Value > 0)
                    {
                        sp.TrangThai = "Còn hàng";
                    }
                    else
                    {
                        sp.TrangThai = "Hết hàng";
                    }
                    sp.FkMaNguoiTao = nguoiDung.MaNguoiDung;
                    dataContext.Add(sp);
                    await dataContext.SaveChangesAsync(); // Lưu để có MaSanPham

                    // Xử lý upload ảnh nếu có
                    var files = Request.Form.Files.Where(f => f.Name == "files" && f.Length > 0).ToList();
                    if (files != null && files.Any())
                    {
                        try
                        {
                            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "sanpham");
                            Directory.CreateDirectory(uploadPath);

                            var savedFiles = new List<string>();

                            foreach (var file in files)
                            {
                                if (file.Length > 0)
                                {
                                    // Kiểm tra định dạng file
                                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                                    
                                    if (!allowedExtensions.Contains(fileExtension))
                                    {
                                        continue; // Bỏ qua file không hợp lệ
                                    }

                                    // Tạo tên file duy nhất: MaSanPham_timestamp_guid
                                    var fileName = $"{sp.MaSanPham}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{fileExtension}";
                                    var path = Path.Combine(uploadPath, fileName);

                                    // Lưu file
                                    using (var stream = new FileStream(path, FileMode.Create))
                                    {
                                        await file.CopyToAsync(stream);
                                    }

                                    // Lưu đường dẫn tương đối (để hiển thị trên web)
                                    var relativePath = $"/uploads/sanpham/{fileName}";
                                    savedFiles.Add(relativePath);
                                }
                            }

                            // Lưu ảnh đầu tiên làm ảnh chính
                            if (savedFiles.Count > 0)
                            {
                                sp.HinhAnh = savedFiles[0];
                                await dataContext.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi nhưng không chặn việc tạo sản phẩm
                            System.Diagnostics.Debug.WriteLine($"Error uploading image: {ex.Message}");
                        }
                    }

                    // Tự động tạo bản ghi tồn kho cho sản phẩm mới
                    // Kiểm tra xem đã có bản ghi tồn kho chưa
                    var tonKhoExist = await dataContext.TonKhos
                        .FirstOrDefaultAsync(t => t.MaSanPham == sp.MaSanPham);
                    
                    if (tonKhoExist == null)
                    {
                        // Lấy IdTonKho lớn nhất và cộng thêm 1
                        int maxId = 0;
                        var hasTonKho = await dataContext.TonKhos.AnyAsync();
                        if (hasTonKho)
                        {
                            maxId = await dataContext.TonKhos
                                .Select(t => t.IdTonKho)
                                .MaxAsync();
                        }
                        
                        var tonKho = new TonKho
                        {
                            IdTonKho = maxId + 1,
                            MaSanPham = sp.MaSanPham,
                            SoLuongHienTai = sp.SoLuong ?? 0,
                            NgayCapNhatCuoi = DateTime.Now
                        };
                        dataContext.TonKhos.Add(tonKho);
                        await dataContext.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Nếu ModelState không hợp lệ, hiển thị lại form với lỗi
                    return View(sp);
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Xử lý lỗi database cụ thể
                string errorMessage = "Lỗi khi lưu sản phẩm vào cơ sở dữ liệu.";
                
                if (dbEx.InnerException != null)
                {
                    var innerEx = dbEx.InnerException;
                    errorMessage += " " + innerEx.Message;
                    
                    // Kiểm tra các lỗi phổ biến
                    if (innerEx.Message.Contains("UNIQUE KEY") || innerEx.Message.Contains("duplicate key"))
                    {
                        errorMessage = "Mã vạch này đã tồn tại. Vui lòng sử dụng mã vạch khác.";
                        ModelState.AddModelError("MaVach", errorMessage);
                    }
                    else if (innerEx.Message.Contains("FOREIGN KEY") || innerEx.Message.Contains("FK__SanPham__FK_MaNg"))
                    {
                        errorMessage = "Người dùng không hợp lệ. Vui lòng đăng nhập lại.";
                        ModelState.AddModelError(string.Empty, errorMessage);
                    }
                    else if (innerEx.Message.Contains("FOREIGN KEY") || innerEx.Message.Contains("FK__SanPham__FK_MaDa"))
                    {
                        errorMessage = "Danh mục không hợp lệ. Vui lòng chọn lại danh mục.";
                        ModelState.AddModelError("FkMaDanhMuc", errorMessage);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, errorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
                
                // Log lỗi chi tiết để debug
                System.Diagnostics.Debug.WriteLine($"Database error: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                }
                
                return View(sp);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tạo sản phẩm: " + e.Message);
                // Log lỗi chi tiết để debug
                System.Diagnostics.Debug.WriteLine($"Error creating product: {e.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {e.InnerException.Message}");
                    ModelState.AddModelError(string.Empty, "Chi tiết lỗi: " + e.InnerException.Message);
                }
                return View(sp);
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
            
            // Populate dropdowns
            ViewBag.DanhMucs = new SelectList(
                dataContext.DanhMucs.ToList(),
                "MaDanhMuc",
                "Ten"
            );
            
            ViewBag.NhaCungCaps = new SelectList(
                dataContext.NhaCungCaps.ToList(),
                "MaNcc",
                "TenNcc"
            );
            
            return View(sp);
        }

        // POST: SanPhams/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham sp)
        {
            if (id != sp.MaSanPham)
                return NotFound();

            // Load entity hiện tại từ database
            var existingSp = await dataContext.SanPhams.FindAsync(id);
            if (existingSp == null)
            {
                return NotFound();
            }

            try
            {
                // Cập nhật từng thuộc tính - luôn cập nhật giá bất kể ModelState
                existingSp.Ten = sp.Ten ?? existingSp.Ten;
                existingSp.MaVach = sp.MaVach;
                
                // Parse giá từ Request.Form (bỏ dấu phẩy và parse)
                var giaBanStr = Request.Form["GiaBan"].ToString().Replace(",", "").Replace(".", "").Trim();
                var giaVonStr = Request.Form["GiaVon"].ToString().Replace(",", "").Replace(".", "").Trim();
                
                // Parse giá bán từ form - LUÔN dùng giá từ form
                if (!string.IsNullOrEmpty(giaBanStr))
                {
                    if (decimal.TryParse(giaBanStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal giaBanParsed))
                    {
                        existingSp.GiaBan = giaBanParsed;
                    }
                }
                
                // Parse giá vốn từ form - Luôn dùng giá từ form
                if (!string.IsNullOrEmpty(giaVonStr))
                {
                    if (decimal.TryParse(giaVonStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal giaVonParsed))
                    {
                        existingSp.GiaVon = giaVonParsed;
                    }
                }
                
                existingSp.SoLuong = sp.SoLuong;
                existingSp.FkMaDanhMuc = sp.FkMaDanhMuc;
                existingSp.MaNcc = sp.MaNcc;
                existingSp.MoTa = sp.MoTa;
                
                // Cập nhật trạng thái (chỉ chấp nhận "Còn hàng" hoặc "Hết hàng")
                if (!string.IsNullOrEmpty(sp.TrangThai))
                {
                    if (sp.TrangThai == "Còn hàng" || sp.TrangThai == "Hết hàng")
                    {
                        existingSp.TrangThai = sp.TrangThai;
                    }
                    else
                    {
                        // Nếu trạng thái không hợp lệ, mặc định là "Còn hàng"
                        existingSp.TrangThai = "Còn hàng";
                    }
                }
                else
                {
                    // Nếu không có trạng thái, mặc định là "Còn hàng"
                    existingSp.TrangThai = "Còn hàng";
                }
                
                // Giữ nguyên các trường không thay đổi
                // existingSp.NgayTao = sp.NgayTao;
                // existingSp.FkMaNguoiTao = sp.FkMaNguoiTao;

                await dataContext.SaveChangesAsync();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await dataContext.SanPhams.AnyAsync(e => e.MaSanPham == id))
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
                ModelState.AddModelError("", "Không lưu được: " + ex.Message);
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
            }

            // Set lại dropdown nếu có lỗi
            ViewBag.DanhMucs = new SelectList(dataContext.DanhMucs.ToList(), "MaDanhMuc", "Ten");
            ViewBag.NhaCungCaps = new SelectList(dataContext.NhaCungCaps.ToList(), "MaNcc", "TenNcc");

            return View(existingSp);
        }
        // GET: SanPhams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await dataContext.SanPhams
                .Include(s => s.FkMaDanhMucNavigation)
                .Include(s => s.MaNccNavigation)
                .Include(s => s.FkMaNguoiTaoNavigation)
                .FirstOrDefaultAsync(m => m.MaSanPham == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // GET: SanPhams/Delete/5

        public IActionResult ByLoai(int id)
        {
            var sp = dataContext.SanPhams
                .Where(x => x.MaLoai == id)
                .ToList();

            return View(sp);
        }
        #region Cập nhật ảnh
        public async Task<IActionResult> UpdateImage(int id)
        {
            var sp = await dataContext.SanPhams.FindAsync(id);
            if (sp == null)
            {
                return NotFound();
            }
            return View(sp);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateImage(int id, List<IFormFile> files)
        {
            var sp = await dataContext.SanPhams.FindAsync(id);
            if (sp == null)
            {
                return NotFound();
            }

            if (files == null || files.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một ảnh.";
                return View(sp);
            }

            try
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "sanpham");
                Directory.CreateDirectory(uploadPath);

                var savedFiles = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Kiểm tra định dạng file
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            TempData["Error"] = $"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}";
                            return View(sp);
                        }

                        // Tạo tên file duy nhất: MaSanPham_timestamp_guid
                        var fileName = $"{id}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{fileExtension}";
                        var path = Path.Combine(uploadPath, fileName);

                        // Lưu file
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn tương đối (để hiển thị trên web)
                        var relativePath = $"/uploads/sanpham/{fileName}";
                        savedFiles.Add(relativePath);
                    }
                }

                // Lưu đường dẫn ảnh vào database
                // Lưu ảnh đầu tiên làm ảnh chính, các ảnh khác có thể lưu vào trường khác hoặc bỏ qua
                if (savedFiles.Count > 0)
                {
                    // Lưu ảnh đầu tiên làm ảnh chính
                    sp.HinhAnh = savedFiles[0];
                    
                    // Nếu có nhiều ảnh, có thể lưu vào một trường khác hoặc chỉ lưu ảnh đầu tiên
                    // Ở đây tôi sẽ chỉ lưu ảnh đầu tiên vào HinhAnh
                    
                    await dataContext.SaveChangesAsync();
                }

                TempData["Success"] = $"Đã tải lên thành công {savedFiles.Count} ảnh. Ảnh đầu tiên đã được đặt làm ảnh chính.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải ảnh: " + ex.Message;
                return View(sp);
            }
        }

        #endregion

        #region Cập nhật trạng thái hàng loạt
        // POST: Admin/SanPham/UpdateAllStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAllStatus()
        {
            try
            {
                // Lấy tất cả sản phẩm có trạng thái không phải "Còn hàng" hoặc "Hết hàng"
                var sanPhams = await dataContext.SanPhams
                    .Where(s => s.TrangThai != "Còn hàng" && s.TrangThai != "Hết hàng")
                    .ToListAsync();

                int countUpdated = 0;
                foreach (var sp in sanPhams)
                {
                    // Nếu số lượng > 0 thì đặt "Còn hàng", ngược lại "Hết hàng"
                    if (sp.SoLuong.HasValue && sp.SoLuong.Value > 0)
                    {
                        sp.TrangThai = "Còn hàng";
                    }
                    else
                    {
                        sp.TrangThai = "Hết hàng";
                    }
                    countUpdated++;
                }

                if (countUpdated > 0)
                {
                    await dataContext.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã cập nhật trạng thái cho {countUpdated} sản phẩm thành công!";
                }
                else
                {
                    TempData["InfoMessage"] = "Không có sản phẩm nào cần cập nhật. Tất cả sản phẩm đã có trạng thái hợp lệ.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

        #region Delete
        // GET: Admin/SanPham/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await dataContext.SanPhams
                .Include(s => s.FkMaDanhMucNavigation)
                .Include(s => s.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaSanPham == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có bản ghi liên quan không
            var tonKhoCount = await dataContext.TonKhos.CountAsync(t => t.MaSanPham == id);
            var chiTietDonHangCount = await dataContext.ChiTietDonHangs.CountAsync(c => c.FkMaSanPham == id);
            var giaoDichKhoCount = await dataContext.GiaoDichKhos.CountAsync(g => g.FkMaSanPham == id);

            ViewBag.TonKhoCount = tonKhoCount;
            ViewBag.ChiTietDonHangCount = chiTietDonHangCount;
            ViewBag.GiaoDichKhoCount = giaoDichKhoCount;
            ViewBag.HasRelatedRecords = tonKhoCount > 0 || chiTietDonHangCount > 0 || giaoDichKhoCount > 0;

            return View(sanPham);
        }

        // POST: Admin/SanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sanPham = await dataContext.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Xóa các bản ghi TonKho liên quan trước
                var tonKhos = await dataContext.TonKhos
                    .Where(t => t.MaSanPham == id)
                    .ToListAsync();
                if (tonKhos.Any())
                {
                    dataContext.TonKhos.RemoveRange(tonKhos);
                }

                // Xóa các bản ghi GiaoDichKho liên quan (nếu có)
                var giaoDichKhos = await dataContext.GiaoDichKhos
                    .Where(g => g.FkMaSanPham == id)
                    .ToListAsync();
                if (giaoDichKhos.Any())
                {
                    dataContext.GiaoDichKhos.RemoveRange(giaoDichKhos);
                }

                // Kiểm tra xem có ChiTietDonHang liên quan không
                var chiTietDonHangs = await dataContext.ChiTietDonHangs
                    .Where(c => c.FkMaSanPham == id)
                    .ToListAsync();
                
                if (chiTietDonHangs.Any())
                {
                    // Nếu có chi tiết đơn hàng, không cho phép xóa
                    TempData["ErrorMessage"] = $"Không thể xóa sản phẩm \"{sanPham.Ten}\" vì đang có {chiTietDonHangs.Count} đơn hàng liên quan!";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa file ảnh nếu có
                if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                {
                    try
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sanPham.HinhAnh.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    catch
                    {
                        // Bỏ qua lỗi xóa file, tiếp tục xóa sản phẩm
                    }
                }

                // Xóa sản phẩm
                dataContext.SanPhams.Remove(sanPham);
                await dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xóa sản phẩm \"{sanPham.Ten}\" thành công!";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                string errorMessage = "Lỗi khi xóa sản phẩm.";
                
                if (dbEx.InnerException != null)
                {
                    var innerEx = dbEx.InnerException;
                    errorMessage += " " + innerEx.Message;
                    
                    if (innerEx.Message.Contains("FOREIGN KEY") || innerEx.Message.Contains("FK_TonKho_SanPham"))
                    {
                        errorMessage = $"Không thể xóa sản phẩm \"{sanPham.Ten}\" vì còn dữ liệu liên quan trong hệ thống. Vui lòng kiểm tra lại.";
                    }
                }
                
                TempData["ErrorMessage"] = errorMessage;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa sản phẩm: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

    }
}
//        public IActionResult Index()

//        {
//            return View();
//        }
//    }
//}
