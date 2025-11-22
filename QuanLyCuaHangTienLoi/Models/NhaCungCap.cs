using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class NhaCungCap
{
    public int MaNcc { get; set; }

    public string TenNcc { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string? MaSoThue { get; set; }

    public string? LoaiHang { get; set; }

    public string? TrangThai { get; set; }

    public DateOnly? NgayHopTac { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
