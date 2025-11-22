using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class ChamCong
{
    public int ChamCongId { get; set; }

    public int FkMaNguoiDung { get; set; }

    public DateOnly Ngay { get; set; }

    public TimeOnly? GioVao { get; set; }

    public TimeOnly? GioRa { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public virtual NguoiDung FkMaNguoiDungNavigation { get; set; } = null!;
}
