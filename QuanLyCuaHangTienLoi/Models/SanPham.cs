using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangTienLoi.Models;

public partial class SanPham
{
    public int MaSanPham { get; set; }

    public string Ten { get; set; } = null!;

    public string? MaVach { get; set; }

    public string? MoTa { get; set; }

    public decimal? TrongLuong { get; set; }

    public decimal GiaBan { get; set; }

    public decimal GiaVon { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? TrangThai { get; set; } = null!;
   
    public int FkMaDanhMuc { get; set; }

    public int FkMaNguoiTao { get; set; }
   
    public int? MaNcc { get; set; }

    public int? MaLoai { get; set; }

    public int? SoLuong { get; set; }

    public int? MaLoaiCon { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietQuetHht> ChiTietQuetHhts { get; set; } = new List<ChiTietQuetHht>();

    public virtual DanhMuc? FkMaDanhMucNavigation { get; set; } = null!;

    public virtual NguoiDung? FkMaNguoiTaoNavigation { get; set; } = null!;

    public virtual ICollection<GiaoDichKho> GiaoDichKhos { get; set; } = new List<GiaoDichKho>();

    public virtual LoaiSanPham? MaLoaiConNavigation { get; set; }

    public virtual NhaCungCap? MaNccNavigation { get; set; }

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
