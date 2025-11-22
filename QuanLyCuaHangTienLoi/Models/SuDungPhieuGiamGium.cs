using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class SuDungPhieuGiamGium
{
    public int MaSuDung { get; set; }

    public int MaPhieu { get; set; }

    public int MaHoaDon { get; set; }

    public int? MaNguoiDung { get; set; }

    public DateTime NgaySuDung { get; set; }

    public virtual HoaDon MaHoaDonNavigation { get; set; } = null!;

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual PhieuGiamGium MaPhieuNavigation { get; set; } = null!;
}
