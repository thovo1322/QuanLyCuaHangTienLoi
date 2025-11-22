using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangTienLoi.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanTriController : Controller
    {
        // GET: Admin/QuanTri
        public IActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("Index", "DangNhap");
            }

            // Redirect đến BangDieuKhien
            return RedirectToAction("Index", "BangDieuKhien");
        }
    }
}
