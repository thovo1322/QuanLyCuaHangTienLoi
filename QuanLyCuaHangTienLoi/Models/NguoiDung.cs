using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? TrangThai { get; set; }

    public string? ChucVu { get; set; }

    public decimal? Luong { get; set; }

    public DateOnly? NgayVaoLam { get; set; }

    public int FkIdQuyenHan { get; set; }

    public int? FkMaChucVu { get; set; }

    public int? FkIdChucVu { get; set; }

    public virtual ICollection<BangLuong> BangLuongs { get; set; } = new List<BangLuong>();

    public virtual ICollection<ChamCong> ChamCongs { get; set; } = new List<ChamCong>();

    public virtual ICollection<DanhMuc> DanhMucs { get; set; } = new List<DanhMuc>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual Chucvu? FkIdChucVuNavigation { get; set; }

    public virtual Quyenhan FkIdQuyenHanNavigation { get; set; } = null!;

    public virtual ICollection<PhienQuetHht> PhienQuetHhts { get; set; } = new List<PhienQuetHht>();

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();

    public virtual ICollection<SuDungPhieuGiamGium> SuDungPhieuGiamGia { get; set; } = new List<SuDungPhieuGiamGium>();
}
