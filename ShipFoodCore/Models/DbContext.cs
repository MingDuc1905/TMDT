using Microsoft.EntityFrameworkCore;

namespace ShipFood.Models;

public partial class dbFoodyEntities : DbContext
{
    // DbSet for raw SQL queries (used by ShipperController)
    public virtual DbSet<DonHangDangLam> DonHangDangLam { get; set; } = null!;
    public dbFoodyEntities(DbContextOptions<dbFoodyEntities> options) : base(options)
    {
    }

    public virtual DbSet<tbAdmin> tbAdmins { get; set; }
    public virtual DbSet<tbBienTheMonAn> tbBienTheMonAns { get; set; }
    public virtual DbSet<tbChiTietDonHang> tbChiTietDonHangs { get; set; }
    public virtual DbSet<tbDanhGia> tbDanhGias { get; set; }
    public virtual DbSet<tbDanhMuc> tbDanhMucs { get; set; }
    public virtual DbSet<tbDonHang> tbDonHangs { get; set; }
    public virtual DbSet<tbKhachHang> tbKhachHangs { get; set; }
    public virtual DbSet<tbKhuyenMai> tbKhuyenMais { get; set; }
    public virtual DbSet<tbLoaiHinhThanhToan> tbLoaiHinhThanhToans { get; set; }
    public virtual DbSet<tbMonAn> tbMonAns { get; set; }
    public virtual DbSet<tbMonAnKhuyenMai> tbMonAnKhuyenMais { get; set; }
    public virtual DbSet<tbQuanAn> tbQuanAns { get; set; }
    public virtual DbSet<tbShipper> tbShippers { get; set; }
    public virtual DbSet<tbThongTinDatHang> tbThongTinDatHangs { get; set; }
    public virtual DbSet<tbTinNhan> tbTinNhans { get; set; }
    public virtual DbSet<tbLichSuSuDungKhuyenMai> tbLichSuSuDungKhuyenMais { get; set; }
    public virtual DbSet<tbUser> tbUsers { get; set; }
    public virtual DbSet<tbEInvoice> tbEInvoices { get; set; }

    // Singular aliases for backward compatibility (DbSet to support Add/Remove/Find)
    public DbSet<tbMonAn> tbMonAn => tbMonAns;
    public DbSet<tbBienTheMonAn> tbBienTheMonAn => tbBienTheMonAns;
    public DbSet<tbQuanAn> tbQuanAn => tbQuanAns;
    public DbSet<tbUser> tbUser => tbUsers;
    public DbSet<tbDanhMuc> tbDanhMuc => tbDanhMucs;
    public DbSet<tbShipper> tbShipper => tbShippers;
    public DbSet<tbChiTietDonHang> tbChiTietDonHang => tbChiTietDonHangs;
    public DbSet<tbKhachHang> tbKhachHang => tbKhachHangs;
    public DbSet<tbDonHang> tbDonHang => tbDonHangs;
    public DbSet<tbKhuyenMai> tbKhuyenMai => tbKhuyenMais;
    public DbSet<tbLoaiHinhThanhToan> tbLoaiHinhThanhToan => tbLoaiHinhThanhToans;
    public DbSet<tbThongTinDatHang> tbThongTinDatHang => tbThongTinDatHangs;
    public DbSet<tbAdmin> tbAdmin => tbAdmins;
    public DbSet<tbMonAnKhuyenMai> tbMonAnKhuyenMai => tbMonAnKhuyenMais;
    public DbSet<tbDanhGia> tbDanhGia => tbDanhGias;
    public DbSet<tbLichSuSuDungKhuyenMai> tbLichSuSuDungKhuyenMai => tbLichSuSuDungKhuyenMais;
    public DbSet<tbEInvoice> tbEInvoice => tbEInvoices;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // tbUser -> tbAdmin (1:1)
        modelBuilder.Entity<tbAdmin>()
            .HasOne(a => a.tbUser)
            .WithOne(u => u.tbAdmin)
            .HasForeignKey<tbAdmin>(a => a.userid)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbUser -> tbKhachHang (1:1)
        modelBuilder.Entity<tbKhachHang>()
            .HasOne(k => k.tbUser)
            .WithOne(u => u.tbKhachHang)
            .HasForeignKey<tbKhachHang>(k => k.userid)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbUser -> tbQuanAn (1:1)
        modelBuilder.Entity<tbQuanAn>()
            .HasOne(q => q.tbUser)
            .WithOne(u => u.tbQuanAn)
            .HasForeignKey<tbQuanAn>(q => q.userid)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbUser -> tbShipper (1:1)
        modelBuilder.Entity<tbShipper>()
            .HasOne(s => s.tbUser)
            .WithOne(u => u.tbShipper)
            .HasForeignKey<tbShipper>(s => s.userid)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbQuanAn -> tbMonAn (1:N)
        // ⚠️ RESTRICT: Chuyển từ CASCADE sang RESTRICT để bảo vệ soft-delete
        // Khi Quán ăn bị xóa, món ăn chỉ được đánh dấu isDeleted = true thay vì xóa cứng
        modelBuilder.Entity<tbMonAn>()
            .HasOne(m => m.tbQuanAn)
            .WithMany(q => q.tbMonAns)
            .HasForeignKey(m => m.maquanan)
            .OnDelete(DeleteBehavior.Restrict);

        // tbDanhMuc -> tbMonAn (1:N)
        // ⚠️ RESTRICT: Không cho phép xóa danh mục nếu còn món ăn
        modelBuilder.Entity<tbMonAn>()
            .HasOne(m => m.tbDanhMuc)
            .WithMany(d => d.tbMonAns)
            .HasForeignKey(m => m.madanhmuc)
            .OnDelete(DeleteBehavior.Restrict);

        // tbMonAn -> tbBienTheMonAn (1:N) — MỚI
        modelBuilder.Entity<tbBienTheMonAn>()
            .HasOne(b => b.tbMonAn)
            .WithMany(m => m.tbBienTheMonAns)
            .HasForeignKey(b => b.mamon)
            .OnDelete(DeleteBehavior.Cascade);

        // tbDonHang -> tbChiTietDonHang (1:N)
        modelBuilder.Entity<tbChiTietDonHang>()
            .HasOne(c => c.tbDonHang)
            .WithMany(d => d.tbChiTietDonHangs)
            .HasForeignKey(c => c.madh)
            .OnDelete(DeleteBehavior.Cascade);

        // tbBienTheMonAn -> tbChiTietDonHang (1:N) — SỬA: mamon → tbBienTheMonAn.id
        modelBuilder.Entity<tbChiTietDonHang>()
            .HasOne(c => c.tbBienTheMonAn)
            .WithMany(b => b.tbChiTietDonHangs)
            .HasForeignKey(c => c.mamon)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbChiTietDonHang -> tbDanhGia (1:N)
        modelBuilder.Entity<tbDanhGia>()
            .HasOne(d => d.tbChiTietDonHang)
            .WithMany(c => c.tbDanhGias)
            .HasForeignKey(d => d.mactdh)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbKhachHang -> tbThongTinDatHang (1:N)
        modelBuilder.Entity<tbThongTinDatHang>()
            .HasOne(t => t.tbKhachHang)
            .WithMany(k => k.tbThongTinDatHangs)
            .HasForeignKey(t => t.userid)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbKhachHang -> tbTinNhan (1:N)
        modelBuilder.Entity<tbTinNhan>()
            .HasOne(t => t.tbKhachHang)
            .WithMany(k => k.tbTinNhans)
            .HasForeignKey(t => t.makh)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbShipper -> tbTinNhan (1:N)
        modelBuilder.Entity<tbTinNhan>()
            .HasOne(t => t.tbShipper)
            .WithMany(s => s.tbTinNhans)
            .HasForeignKey(t => t.mashipper)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbDonHang -> tbTinNhan (1:N)
        modelBuilder.Entity<tbTinNhan>()
            .HasOne(t => t.tbDonHang)
            .WithMany(d => d.tbTinNhans)
            .HasForeignKey(t => t.madh)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbDonHang -> tbLoaiHinhThanhToan (N:1)
        modelBuilder.Entity<tbDonHang>()
            .HasOne(d => d.tbLoaiHinhThanhToan)
            .WithMany(l => l.tbDonHangs)
            .HasForeignKey(d => d.hinhthucthanhtoan)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbDonHang -> tbQuanAn (N:1)
        modelBuilder.Entity<tbDonHang>()
            .HasOne(d => d.tbQuanAn)
            .WithMany(q => q.tbDonHangs)
            .HasForeignKey(d => d.maquan)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbDonHang -> tbKhuyenMai (N:1)
        modelBuilder.Entity<tbDonHang>()
            .HasOne(d => d.tbKhuyenMai)
            .WithMany(k => k.tbDonHangs)
            .HasForeignKey(d => d.makhuyenmai)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbDonHang -> tbShipper (N:1)
        modelBuilder.Entity<tbDonHang>()
            .HasOne(d => d.tbShipper)
            .WithMany(s => s.tbDonHangs)
            .HasForeignKey(d => d.mashipper)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbDonHang -> tbThongTinDatHang (N:1)
        modelBuilder.Entity<tbDonHang>()
            .HasOne(d => d.tbThongTinDatHang)
            .WithMany(t => t.tbDonHangs)
            .HasForeignKey(d => d.mattdh)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbBienTheMonAn -> tbMonAnKhuyenMai (1:N) — SỬA: mamon → tbBienTheMonAn.id
        modelBuilder.Entity<tbMonAnKhuyenMai>()
            .HasOne(m => m.tbBienTheMonAn)
            .WithMany(b => b.tbMonAnKhuyenMais)
            .HasForeignKey(m => m.mamon)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // tbKhuyenMai -> tbMonAnKhuyenMai (1:N)
        modelBuilder.Entity<tbMonAnKhuyenMai>()
            .HasOne(m => m.tbKhuyenMai)
            .WithMany(k => k.tbMonAnKhuyenMais)
            .HasForeignKey(m => m.makm)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
