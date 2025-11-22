using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class GiaoDichKho
{
    public int MaGiaoDich { get; set; }

    public string LoaiGiaoDich { get; set; } = null!;

    public int SoLuong { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? TrangThai { get; set; }

    public int FkMaSanPham { get; set; }

    public int? FkMaDonHang { get; set; }

    public virtual DonHang? FkMaDonHangNavigation { get; set; }

    public virtual SanPham FkMaSanPhamNavigation { get; set; } = null!;
}
