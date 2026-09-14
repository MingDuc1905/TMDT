// ============================================================
// 🏘️ District — Quận/huyện (dữ liệu tĩnh, dùng cùng City)
// ============================================================
// Ý nghĩa: Model tĩnh đơn giản chứa tên 1 quận/huyện
// Chức năng: Được City khởi tạo danh sách mặc định cho dropdown địa chỉ
// KEYWORDS: district, quan, huyen, dia chi, address
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: City.cs (constructor tạo List<District>), Views dùng
//     dropdown chọn quận khi nhập địa chỉ giao hàng
// ============================================================
namespace ShipFood.Models;

public class District
{
    public string nameDistrict { get; set; }

    public District(string name)
    {
        nameDistrict = name;
    }
}
