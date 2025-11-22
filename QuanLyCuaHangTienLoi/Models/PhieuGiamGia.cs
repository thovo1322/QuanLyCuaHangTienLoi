using System;
using System.Collections.Generic;

namespace QuanLyCuaHangTienLoi.Models;

public partial class PhieuGiamGium
{
    public int MaPhieu { get; set; }

    public string TenPhieu { get; set; } = null!;

    public string MaPhieuCode { get; set; } = null!;

    public decimal GiaTriGiam { get; set; }

    public DateTime NgayHetHan { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<SuDungPhieuGiamGium> SuDungPhieuGiamGia { get; set; } = new List<SuDungPhieuGiamGium>();
}
