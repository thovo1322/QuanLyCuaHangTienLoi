namespace QuanLyCuaHangTienLoi.Models
{
    public class SeedData
    {
        public static void SeedingData(QlchtlContext context)
        {
            // Nếu bảng loại sản phẩm chưa có dữ liệu thì thêm vào
            if (!context.DanhMucs.Any())
            {
         
                context.DanhMucs.AddRange(
                    new DanhMuc { MaDanhMuc = 4, Ten = "Đồ uống", TrangThai = "Hoạt động" },
                    new DanhMuc { MaDanhMuc = 5, Ten = "Thực phẩm khô", TrangThai = "Hoạt động" },
                    new DanhMuc { MaDanhMuc = 7, Ten = "Đồ ăn nhanh", TrangThai = "Hoạt động" }
                );
                context.SaveChanges();
            }

            // Nếu bảng sản phẩm trống thì thêm vài sản phẩm mẫu
            // Tạm thời comment để tránh lỗi SoLuong
            /*
            if (!context.SanPhams.Any())
            {
                var doAn = context.LoaiSanPhams.FirstOrDefault(l => l.TenLoai == "Đồ ăn");
                var thucUong = context.LoaiSanPhams.FirstOrDefault(l => l.TenLoai == "Thức uống");

                if (doAn != null && thucUong != null)
                {
                    context.SanPhams.AddRange(
                        new SanPham { Ten = "Bánh mì sandwich", GiaBan = 15000, MaLoai = doAn.MaLoai, TrangThai = "Hoạt động" },
                        new SanPham { Ten = "Sữa tươi Vinamilk", GiaBan = 12000, MaLoai = thucUong.MaLoai, TrangThai = "Hoạt động" },
                        new SanPham { Ten = "Mì gói Hảo Hảo", GiaBan = 4000, MaLoai = doAn.MaLoai, TrangThai = "Hoạt động" }
                    );
                    context.SaveChanges();
                }
            }
            */

            // Tạo các chức vụ
            if (!context.Chucvus.Any())
            {
                context.Chucvus.AddRange(
                    new Chucvu { IdChucvu = 1, TenVaiTro = "Quản lý cửa hàng" },
                    new Chucvu { IdChucvu = 2, TenVaiTro = "Nhân viên cửa hàng" }
                );
                context.SaveChanges();
            }

            // Thêm dữ liệu admin để đăng nhập
            if (!context.NhanViens.Any())
            {
                // Tạo nhân viên admin với chức vụ "Quản lý cửa hàng"
                var adminUser = new NhanVien
                {
                    MaNv = 1,
                    TenNv = "admin",
                    MatKhauHash = HashPassword("admin123"), // Mật khẩu: admin123
                    IdChucvu = 1, // Quản lý cửa hàng
                    Luongcoban = 10000000,
                    Phucap = 2000000
                };
                context.NhanViens.Add(adminUser);

                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(hashedBytes).ToLower();
            }
        }
    }
}
