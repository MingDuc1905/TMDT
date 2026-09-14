// ============================================================
// 🏙️ City — Thành phố + danh sách quận/huyện (dữ liệu tĩnh)
// ============================================================
// Ý nghĩa: Model tĩnh (không phải bảng DB) mô tả TP.HCM kèm danh sách quận
// Chức năng: Constructor tự sinh 13 quận/huyện mặc định cho form chọn địa chỉ
// KEYWORDS: city, thanh pho, quan, huyen, dia chi, address, district
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: District.cs (District — 1 quận), HomeController
//     (ViewBag/City dùng cho dropdown chọn địa chỉ giao hàng),
//     Views Checkout/Đăng ký (dropdown quận/huyện)
//   → LƯU Ý: dữ liệu CỨNG trong code — muốn thêm quận phải sửa file này
// ============================================================
namespace ShipFood.Models;

public class City
{
    public City()
    {
        nameCity = "TP. Hồ Chí Minh";
        districts = new List<District>();
        string[] names = {
            "Quận 1", "Quận 3", "Quận 5", "Quận 7", "Quận 10",
            "Bình Thạnh", "Tân Bình", "Gò Vấp", "Phú Nhuận",
            "Thủ Đức", "Bình Dương", "Hóc Môn", "Củ Chi"
        };
        foreach (string n in names)
        {
            districts.Add(new District(n));
        }
    }

    public string nameCity { get; set; } = "TP. Hồ Chí Minh";
    public List<District> districts { get; set; } = new();
}
