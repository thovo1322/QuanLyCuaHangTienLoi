using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class Chucvu
{
    public int IdChucvu { get; set; }

    public string TenVaiTro { get; set; } = null!;

    public virtual ICollection<ChucvuQuyenhan> ChucvuQuyenhans { get; set; } = new List<ChucvuQuyenhan>();

    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
