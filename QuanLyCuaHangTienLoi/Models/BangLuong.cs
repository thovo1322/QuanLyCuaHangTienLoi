using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class BangLuong
{
    public int BangLuongId { get; set; }

    public int FkMaNguoiDung { get; set; }

    public int Thang { get; set; }

    public int Nam { get; set; }

    public int? SoNgayCong { get; set; }

    public int? SoNgayNghi { get; set; }

    public decimal? TongGioLam { get; set; }

    public decimal? LuongCoBan { get; set; }

    public decimal? TongLuong { get; set; }

    public DateOnly? NgayTinhLuong { get; set; }

    public virtual NguoiDung FkMaNguoiDungNavigation { get; set; } = null!;
}
