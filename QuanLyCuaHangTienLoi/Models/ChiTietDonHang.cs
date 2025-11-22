using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class ChiTietDonHang
{
    public int FkMaDonHang { get; set; }

    public int FkMaSanPham { get; set; }

    public int SoLuong { get; set; }

    public decimal GiaBanLucDo { get; set; }

    public decimal GiamGia { get; set; }

    public decimal Thue { get; set; }

    public string? TrangThai { get; set; }

    public virtual DonHang FkMaDonHangNavigation { get; set; } = null!;

    public virtual SanPham FkMaSanPhamNavigation { get; set; } = null!;
}
