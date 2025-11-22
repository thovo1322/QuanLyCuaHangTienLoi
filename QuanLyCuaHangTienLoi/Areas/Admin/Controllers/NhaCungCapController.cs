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
        
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả Nhà Cung Cấp
            var nhaCungCaps = await dataContext.NhaCungCaps.ToListAsync();

            // Nếu bạn gặp cảnh báo CS8602, bạn có thể dùng: dataContext.NhaCungCaps!.ToListAsync();

            return View(nhaCungCaps);
        }
    }
}
