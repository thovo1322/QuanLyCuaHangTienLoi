using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class NhanVien
{
    public int MaNv { get; set; }

    public string? TenNv { get; set; }

    public string MatKhauHash { get; set; } = null!;

    public int IdChucvu { get; set; }

    public DateOnly? Ngayvaolam { get; set; }

    public DateOnly? Ngaynghiviec { get; set; }

    public decimal? Luongcoban { get; set; }

    public decimal? Phucap { get; set; }

    public virtual Chucvu IdChucvuNavigation { get; set; } = null!;
}
