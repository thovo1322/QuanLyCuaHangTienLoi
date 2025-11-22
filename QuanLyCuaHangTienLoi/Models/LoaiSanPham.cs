using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class LoaiSanPham
{
    public int MaLoai { get; set; }

    public string? TenLoai { get; set; }

    public int? MaDanhMuc { get; set; }

    public virtual DanhMuc? MaDanhMucNavigation { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
