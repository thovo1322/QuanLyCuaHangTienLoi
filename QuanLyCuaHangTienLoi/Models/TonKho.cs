using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class TonKho
{
    public int IdTonKho { get; set; }

    public int SoLuongHienTai { get; set; }

    public DateTime? NgayCapNhatCuoi { get; set; }

    public int? MaSanPham { get; set; }

    public int? MaLoai { get; set; }

    public virtual SanPham? MaSanPhamNavigation { get; set; }
}
