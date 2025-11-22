using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class Quyenhan
{
    public int IdQuyenhan { get; set; }

    public string TenQuyenHan { get; set; } = null!;

    public string? MôTả { get; set; }

    public virtual ICollection<ChucvuQuyenhan> ChucvuQuyenhans { get; set; } = new List<ChucvuQuyenhan>();

    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}
