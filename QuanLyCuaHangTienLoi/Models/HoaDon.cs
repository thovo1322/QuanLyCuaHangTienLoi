using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class HoaDon
{
    public int MaHd { get; set; }

    public string MaGd { get; set; } = null!;

    public DateTime NgayThanhToan { get; set; }

    public decimal TongTien { get; set; }

    public decimal GiamGia { get; set; }

    public decimal TongHoaDon { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? PhuongThucThanhToan { get; set; }

    public string MaNv { get; set; } = null!;

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDonMaHangHoaNavigations { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDonMaHdNavigations { get; set; } = new List<ChiTietHoaDon>();

    public virtual SuDungPhieuGiamGium? SuDungPhieuGiamGium { get; set; }
}
