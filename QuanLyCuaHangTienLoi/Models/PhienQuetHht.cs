using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class PhienQuetHht
{
    public int MaPhien { get; set; }

    public string TenPhien { get; set; } = null!;

    public DateTime? NgayTao { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayTaiLen { get; set; }

    public int FkMaNv { get; set; }

    public virtual ICollection<ChiTietQuetHht> ChiTietQuetHhts { get; set; } = new List<ChiTietQuetHht>();

    public virtual NguoiDung FkMaNvNavigation { get; set; } = null!;
}
