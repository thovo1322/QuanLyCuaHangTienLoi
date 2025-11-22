using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class ChucvuQuyenhan
{
    public int IdChucvuQuyenhan { get; set; }

    public int IdChucvu { get; set; }

    public int IdQuyenhan { get; set; }

    public virtual Chucvu IdChucvuNavigation { get; set; } = null!;

    public virtual Quyenhan IdQuyenhanNavigation { get; set; } = null!;
}
