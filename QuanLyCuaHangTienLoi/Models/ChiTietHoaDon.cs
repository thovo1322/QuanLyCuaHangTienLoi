using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class ChiTietHoaDon
{
    public int MaHd { get; set; }

    public int MaHangHoa { get; set; }

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal GiamGia { get; set; }

    public string? TrangThai { get; set; }

    public virtual HoaDon MaHangHoaNavigation { get; set; } = null!;

    public virtual HoaDon MaHdNavigation { get; set; } = null!;
}
