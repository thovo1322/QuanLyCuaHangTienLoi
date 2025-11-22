using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyCuaHangTienLoi.Models;

public partial class QlchtlContext : DbContext
{
    public QlchtlContext()
    {
    }

    public QlchtlContext(DbContextOptions<QlchtlContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BangLuong> BangLuongs { get; set; }

    public virtual DbSet<ChamCong> ChamCongs { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<ChiTietQuetHht> ChiTietQuetHhts { get; set; }

    public virtual DbSet<Chucvu> Chucvus { get; set; }

    public virtual DbSet<ChucvuQuyenhan> ChucvuQuyenhans { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GiaoDichKho> GiaoDichKhos { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<LoaiSanPham> LoaiSanPhams { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhienQuetHht> PhienQuetHhts { get; set; }

    public virtual DbSet<PhieuGiamGium> PhieuGiamGia { get; set; }

    public virtual DbSet<Quyenhan> Quyenhans { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<SuDungPhieuGiamGium> SuDungPhieuGiamGia { get; set; }

    public virtual DbSet<TonKho> TonKhos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-80RHJMF\\THOMSSQLSERVER;Database=QLCHTL;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BangLuong>(entity =>
        {
            entity.HasKey(e => e.BangLuongId).HasName("PK__BangLuon__2BC511E06AED5C56");

            entity.ToTable("BangLuong");

            entity.Property(e => e.BangLuongId).HasColumnName("BangLuong_Id");
            entity.Property(e => e.FkMaNguoiDung).HasColumnName("FK_MaNguoiDung");
            entity.Property(e => e.LuongCoBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NgayTinhLuong).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TongGioLam).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TongLuong).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.FkMaNguoiDungNavigation).WithMany(p => p.BangLuongs)
                .HasForeignKey(d => d.FkMaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BangLuong__FK_Ma__00200768");
        });

        modelBuilder.Entity<ChamCong>(entity =>
        {
            entity.HasKey(e => e.ChamCongId).HasName("PK__ChamCong__C9A84196F4AB063E");

            entity.ToTable("ChamCong");

            entity.Property(e => e.ChamCongId).HasColumnName("ChamCong_Id");
            entity.Property(e => e.FkMaNguoiDung).HasColumnName("FK_MaNguoiDung");
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.FkMaNguoiDungNavigation).WithMany(p => p.ChamCongs)
                .HasForeignKey(d => d.FkMaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChamCong__FK_MaN__7C4F7684");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.FkMaDonHang, e.FkMaSanPham }).HasName("PK__ChiTietD__A025906E979B731B");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.FkMaDonHang).HasColumnName("FK_MaDonHang");
            entity.Property(e => e.FkMaSanPham).HasColumnName("FK_MaSanPham");
            entity.Property(e => e.GiaBanLucDo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Thue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.FkMaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.FkMaDonHang)
                .HasConstraintName("FK__ChiTietDo__FK_Ma__30C33EC3");

            entity.HasOne(d => d.FkMaSanPhamNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.FkMaSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__FK_Ma__31B762FC");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => new { e.MaHd, e.MaHangHoa });

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.MaHangHoaNavigation).WithMany(p => p.ChiTietHoaDonMaHangHoaNavigations)
                .HasForeignKey(d => d.MaHangHoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietHoaDon_HangHoa");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ChiTietHoaDonMaHdNavigations)
                .HasForeignKey(d => d.MaHd)
                .HasConstraintName("FK_ChiTietHoaDon_HoaDon");
        });

        modelBuilder.Entity<ChiTietQuetHht>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietQ__CDF0A114FA7B5CCB");

            entity.ToTable("ChiTietQuet_HHT");

            entity.Property(e => e.FkMaPhien).HasColumnName("FK_MaPhien");
            entity.Property(e => e.FkMaSanPham).HasColumnName("FK_MaSanPham");
            entity.Property(e => e.ThoiGianQuet)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.FkMaPhienNavigation).WithMany(p => p.ChiTietQuetHhts)
                .HasForeignKey(d => d.FkMaPhien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietQu__FK_Ma__1BC821DD");

            entity.HasOne(d => d.FkMaSanPhamNavigation).WithMany(p => p.ChiTietQuetHhts)
                .HasForeignKey(d => d.FkMaSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietQu__FK_Ma__1CBC4616");
        });

        modelBuilder.Entity<Chucvu>(entity =>
        {
            entity.HasKey(e => e.IdChucvu).HasName("PK__Chucvu__64B365650E1BB03A");

            entity.ToTable("Chucvu");

            entity.HasIndex(e => e.TenVaiTro, "UQ__Chucvu__1DA55814632BA71A").IsUnique();

            entity.Property(e => e.IdChucvu)
                .ValueGeneratedNever()
                .HasColumnName("ID_Chucvu");
            entity.Property(e => e.TenVaiTro)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ChucvuQuyenhan>(entity =>
        {
            entity.HasKey(e => e.IdChucvuQuyenhan).HasName("PK__Chucvu_Q__F6710E3072C77DC2");

            entity.ToTable("Chucvu_Quyenhan");

            entity.HasIndex(e => new { e.IdChucvu, e.IdQuyenhan }, "UQ_Chucvu_Quyenhan").IsUnique();

            entity.Property(e => e.IdChucvuQuyenhan)
                .ValueGeneratedNever()
                .HasColumnName("ID_Chucvu_Quyenhan");
            entity.Property(e => e.IdChucvu).HasColumnName("ID_Chucvu");
            entity.Property(e => e.IdQuyenhan).HasColumnName("ID_Quyenhan");

            entity.HasOne(d => d.IdChucvuNavigation).WithMany(p => p.ChucvuQuyenhans)
                .HasForeignKey(d => d.IdChucvu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chucvu_Qu__ID_Ch__5CD6CB2B");

            entity.HasOne(d => d.IdQuyenhanNavigation).WithMany(p => p.ChucvuQuyenhans)
                .HasForeignKey(d => d.IdQuyenhan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chucvu_Qu__ID_Qu__5DCAEF64");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMuc__B375088783C33A81");

            entity.ToTable("DanhMuc");

            entity.Property(e => e.FkMaNguoiTao).HasColumnName("FK_MaNguoiTao");
            entity.Property(e => e.Ten).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.FkMaNguoiTaoNavigation).WithMany(p => p.DanhMucs)
                .HasForeignKey(d => d.FkMaNguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhMuc__FK_MaNg__06CD04F7");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584ADBAD942F4");

            entity.ToTable("DonHang");

            entity.HasIndex(e => e.MaHoaDon, "UQ__DonHang__835ED13AB9418548").IsUnique();

            entity.Property(e => e.FkMaNv).HasColumnName("FK_MaNV");
            entity.Property(e => e.MaHoaDon).HasMaxLength(50);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.TongTienGiamGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienSauGiam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienTruocGiam).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.FkMaNvNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.FkMaNv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__FK_MaNV__22751F6C");
        });

        modelBuilder.Entity<GiaoDichKho>(entity =>
        {
            entity.HasKey(e => e.MaGiaoDich).HasName("PK__GiaoDich__0A2A24EBB9A8DF0D");

            entity.ToTable("GiaoDichKho");

            entity.Property(e => e.FkMaDonHang).HasColumnName("FK_MaDonHang");
            entity.Property(e => e.FkMaSanPham).HasColumnName("FK_MaSanPham");
            entity.Property(e => e.LoaiGiaoDich).HasMaxLength(50);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.FkMaDonHangNavigation).WithMany(p => p.GiaoDichKhos)
                .HasForeignKey(d => d.FkMaDonHang)
                .HasConstraintName("FK_GiaoDichKho_DonHang");

            entity.HasOne(d => d.FkMaSanPhamNavigation).WithMany(p => p.GiaoDichKhos)
                .HasForeignKey(d => d.FkMaSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiaoDichK__FK_Ma__2B0A656D");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HoaDon__2725A6E085371B3B");

            entity.ToTable("HoaDon");

            entity.HasIndex(e => e.MaGd, "UQ__HoaDon__2725AE80CAA17B6E").IsUnique();

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaGd)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaGD");
            entity.Property(e => e.MaNv)
                .HasMaxLength(50)
                .HasColumnName("MaNV");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayThanhToan).HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TongHoaDon).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LoaiSanPham>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK__LoaiCon__91538C1AFF284662");

            entity.ToTable("LoaiSanPham");

            entity.Property(e => e.TenLoai).HasMaxLength(100);

            entity.HasOne(d => d.MaDanhMucNavigation).WithMany(p => p.LoaiSanPhams)
                .HasForeignKey(d => d.MaDanhMuc)
                .HasConstraintName("FK__LoaiSanPham__MaDanhM__625A9A57");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762B5631B6F");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.TenDangNhap, "UQ__NguoiDun__55F68FC0C19374AD").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D105343A68940C").IsUnique();

            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FkIdChucVu).HasColumnName("FK_Id_ChucVu");
            entity.Property(e => e.FkIdQuyenHan).HasColumnName("FK_Id_QuyenHan");
            entity.Property(e => e.FkMaChucVu).HasColumnName("FK_MaChucVu");
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.Luong).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.FkIdChucVuNavigation).WithMany(p => p.NguoiDungs)
                .HasForeignKey(d => d.FkIdChucVu)
                .HasConstraintName("FK_NguoiDung_ChucVu");

            entity.HasOne(d => d.FkIdQuyenHanNavigation).WithMany(p => p.NguoiDungs)
                .HasForeignKey(d => d.FkIdQuyenHan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NguoiDung__FK_Id__787EE5A0");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK__NhaCungC__3A185DEB3DFC460F");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.MaNcc).HasColumnName("MaNCC");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.LoaiHang).HasMaxLength(100);
            entity.Property(e => e.MaSoThue).HasMaxLength(50);
            entity.Property(e => e.NgayHopTac).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TenNcc)
                .HasMaxLength(100)
                .HasColumnName("TenNCC");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Đang hợp tác");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNv).HasName("PK__NhanVien__2725D70A073CB9E1");

            entity.ToTable("NhanVien");

            entity.Property(e => e.MaNv)
                .ValueGeneratedNever()
                .HasColumnName("MaNV");
            entity.Property(e => e.IdChucvu).HasColumnName("ID_Chucvu");
            entity.Property(e => e.Luongcoban).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phucap).HasColumnType("decimal(15, 0)");
            entity.Property(e => e.TenNv)
                .HasMaxLength(100)
                .HasColumnName("TenNV");

            entity.HasOne(d => d.IdChucvuNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.IdChucvu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__ID_Chu__3D5E1FD2");
        });

        modelBuilder.Entity<PhienQuetHht>(entity =>
        {
            entity.HasKey(e => e.MaPhien).HasName("PK__PhienQue__2660BFEF283FC135");

            entity.ToTable("PhienQuet_HHT");

            entity.Property(e => e.FkMaNv).HasColumnName("FK_MaNV");
            entity.Property(e => e.NgayTaiLen).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenPhien).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.FkMaNvNavigation).WithMany(p => p.PhienQuetHhts)
                .HasForeignKey(d => d.FkMaNv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhienQuet__FK_Ma__03F0984C");
        });

        modelBuilder.Entity<PhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.MaPhieu).HasName("PK__PhieuGia__2660BFE0B86ADE71");

            entity.HasIndex(e => e.MaPhieuCode, "UQ__PhieuGia__7678CA65427D78BC").IsUnique();

            entity.Property(e => e.GiaTriGiam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaPhieuCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayHetHan).HasColumnType("datetime");
            entity.Property(e => e.TenPhieu).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Quyenhan>(entity =>
        {
            entity.HasKey(e => e.IdQuyenhan).HasName("PK__Quyenhan__DFD6DC2AAB0AD1BF");

            entity.ToTable("Quyenhan");

            entity.HasIndex(e => e.TenQuyenHan, "UQ__Quyenhan__DC5279DA80302B35").IsUnique();

            entity.Property(e => e.IdQuyenhan)
                .ValueGeneratedNever()
                .HasColumnName("ID_Quyenhan");
            entity.Property(e => e.MôTả).HasColumnType("text");
            entity.Property(e => e.TenQuyenHan)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSanPham).HasName("PK__SanPham__FAC7442D6F1CD557");

            entity.ToTable("SanPham");

            entity.HasIndex(e => e.MaVach, "UQ__SanPham__8BBF4A1CD45FBCA5").IsUnique();

            entity.Property(e => e.FkMaDanhMuc).HasColumnName("FK_MaDanhMuc");
            entity.Property(e => e.FkMaNguoiTao).HasColumnName("FK_MaNguoiTao");
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaVon).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaNcc).HasColumnName("MaNCC");
            entity.Property(e => e.MaVach).HasMaxLength(50);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasMaxLength(20);
            entity.Property(e => e.TrongLuong).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.FkMaDanhMucNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.FkMaDanhMuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SanPham__FK_MaDa__17036CC0");

            entity.HasOne(d => d.FkMaNguoiTaoNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.FkMaNguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SanPham__FK_MaNg__17F790F9");

            entity.HasOne(d => d.MaLoaiConNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaLoaiCon)
                .HasConstraintName("FK__SanPham__MaLoaiC__634EBE90");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaNcc)
                .HasConstraintName("FK_SanPham_NhaCungCap");
        });

        modelBuilder.Entity<SuDungPhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.MaSuDung).HasName("PK__SuDungPh__73EF96E9F0AF7AA4");

            entity.HasIndex(e => e.MaHoaDon, "UQ__SuDungPh__835ED13A981D1D93").IsUnique();

            entity.Property(e => e.NgaySuDung).HasColumnType("datetime");

            entity.HasOne(d => d.MaHoaDonNavigation).WithOne(p => p.SuDungPhieuGiamGium)
                .HasForeignKey<SuDungPhieuGiamGium>(d => d.MaHoaDon)
                .HasConstraintName("FK_SuDungPhieuGiamGia_HoaDon");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.SuDungPhieuGiamGia)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK_SuDungPhieuGiamGia_NguoiDung");

            entity.HasOne(d => d.MaPhieuNavigation).WithMany(p => p.SuDungPhieuGiamGia)
                .HasForeignKey(d => d.MaPhieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SuDungPhieuGiamGia_Phieu");
        });

        modelBuilder.Entity<TonKho>(entity =>
        {
            entity.HasKey(e => e.IdTonKho).HasName("PK__TonKho__BF23FE2A6CA2FD57");

            entity.ToTable("TonKho", tb => tb.HasTrigger("TR_TonKho_Update"));

            entity.Property(e => e.IdTonKho)
                .ValueGeneratedNever()
                .HasColumnName("ID_TonKho");
            entity.Property(e => e.NgayCapNhatCuoi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaSanPhamNavigation).WithMany(p => p.TonKhos)
                .HasForeignKey(d => d.MaSanPham)
                .HasConstraintName("FK_TonKho_SanPham");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
