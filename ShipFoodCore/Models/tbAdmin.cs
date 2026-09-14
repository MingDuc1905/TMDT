// ============================================================
// 🛡️ tbAdmin — Bảng quản trị viên
// ============================================================
// Ý nghĩa: Thông tin hồ sơ Admin (userid = FK → tbUser)
// Chức năng: Tên hiển thị + ảnh đại diện của tài khoản quản trị
// KEYWORDS: admin, quan tri, tai khoan admin, tbAdmin
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: tbUser.cs (FK userid), AdminController (login/phân quyền),
//     RoleGuardMiddleware (chặn role khác vào /Admin)
//   → 1 Admin = 1 dòng tbAdmin + 1 dòng tbUser (loaitaikhoan="Admin")
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbAdmin")]
public partial class tbAdmin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int userid { get; set; }

    [Required]
    [MaxLength(50)]
    public string tenadmin { get; set; } = null!;

    [MaxLength(500)]
    public string? hinhanh { get; set; }

    // Navigation
    public virtual tbUser tbUser { get; set; } = null!;
}
