using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class ChiTietQuetHht
{
    public int MaChiTiet { get; set; }

    public int FkMaPhien { get; set; }

    public int FkMaSanPham { get; set; }

    public int SoLuongDat { get; set; }

    public int? TonKhoHienTai { get; set; }

    public DateTime? ThoiGianQuet { get; set; }

    public virtual PhienQuetHht FkMaPhienNavigation { get; set; } = null!;

    public virtual SanPham FkMaSanPhamNavigation { get; set; } = null!;
}
