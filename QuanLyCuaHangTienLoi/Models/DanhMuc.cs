using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class DanhMuc
{
    public int MaDanhMuc { get; set; }

    public string Ten { get; set; } = null!;

    public string? MoTa { get; set; }

    public string TrangThai { get; set; } = null!;

    public int FkMaNguoiTao { get; set; }

    public virtual NguoiDung FkMaNguoiTaoNavigation { get; set; } = null!;

    public virtual ICollection<LoaiSanPham> LoaiSanPhams { get; set; } = new List<LoaiSanPham>();

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
