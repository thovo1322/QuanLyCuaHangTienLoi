using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class DonHang
{
    public int MaDonHang { get; set; }

    public string? MaHoaDon { get; set; }

    public DateTime? NgayTao { get; set; }

    public decimal TongTienTruocGiam { get; set; }

    public decimal TongTienGiamGia { get; set; }

    public decimal TongTienSauGiam { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public int FkMaNv { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual NguoiDung FkMaNvNavigation { get; set; } = null!;

    public virtual ICollection<GiaoDichKho> GiaoDichKhos { get; set; } = new List<GiaoDichKho>();
}
