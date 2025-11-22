using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangTienLoi.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang chủ Admin";
            return View();
        }
        public IActionResult SanPham()
        {
            ViewData["Title"] = "Quản lý sản phẩm";
            return View();
        }
    }
}

