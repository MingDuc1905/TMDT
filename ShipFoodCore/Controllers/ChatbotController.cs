using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using ShipFood.Models;

namespace ShipFood.Controllers;

public class ChatbotController : BaseController
{
    public ChatbotController(dbFoodyEntities context)
    {
        db = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Json(new { reply = "Vui lòng nhập câu hỏi!", quickReplies = new string[] { } });

        var lowerMsg = message.ToLower().Trim();

        // 1. FAQ
        var faqResult = HandleFAQ(lowerMsg);
        if (faqResult != null) return Json(faqResult);

        // 2. Tra cứu đơn hàng
        var orderResult = HandleOrderLookup(lowerMsg);
        if (orderResult != null) return Json(orderResult);

        // 3. Gợi ý món
        var recommendResult = HandleRecommendation(lowerMsg);
        if (recommendResult != null) return Json(recommendResult);

        // 4. Fallback
        return Json(new
        {
            reply = "Xin chao! Toi la tro ly FastShip. Toi co the giup ban:" +
                    "\n- Tra cuu don hang (go ma don)" +
                    "\n- Xem mon an ban chay (go 'goi y')" +
                    "\n- Tra loi cac cau hoi thuong gap",
            quickReplies = new[] { "Phi ship tinh the nao?", "Lam sao de dang ky ban hang?", "Chinh sach huy don?", "Goi y mon an" }
        });
    }

    private object? HandleFAQ(string msg)
    {
        if (ContainsAny(msg, "phi ship", "phi van chuyen", "tien ship"))
        {
            return new
            {
                reply = "Phi van chuyen FastShip:" +
                        "\n- Phi ship co dinh: 15,000 VND" +
                        "\n- Don hang tu 100,000 VND duoc mien phi ship" +
                        "\n- Thoi gian giao: 30-45 phut",
                quickReplies = new[] { "Dang ky ban hang", "Huy don the nao?", "Goi y mon an" }
            };
        }

        if (msg.Contains("dang ky") && (msg.Contains("ban") || msg.Contains("quan") || msg.Contains("nha hang")))
        {
            return new
            {
                reply = "Dang ky ban hang tren FastShip:" +
                        "\n1. Dang ky tai khoan voi loai 'Quan an'" +
                        "\n2. Cho Admin duyet (thuong trong 24h)" +
                        "\n3. Sau khi duyet, dang nhap va quan ly menu" +
                        "\n\nLien he: Fastship@contact.com",
                quickReplies = new[] { "Phi ship", "Chinh sach huy don", "Goi y mon an" }
            };
        }

        if (ContainsAny(msg, "huy don", "huỷ don", "cancel"))
        {
            return new
            {
                reply = "Chinh sach huy don:" +
                        "\n- Don hang co the huy trong 5 phut sau khi dat" +
                        "\n- Neu quan da xac nhan, lien he quan de huy" +
                        "\n- Don da giao khong the huy" +
                        "\n\nDe huy don, vao Lich su don hang va chon 'Huy'",
                quickReplies = new[] { "Phi ship", "Dang ky ban hang", "Goi y mon an" }
            };
        }

        if (ContainsAny(msg, "gio", "thoi gian", "mo cua"))
        {
            return new
            {
                reply = "Gio hoat dong:" +
                        "\n- Cac quan mo cua tu 7:00 - 21:30" +
                        "\n- Gio mo cua co the khac nhau tuy quan" +
                        "\n- Xem chi tiet tren trang cua tung quan",
                quickReplies = new[] { "Phi ship", "Dang ky ban hang", "Goi y mon an" }
            };
        }

        if (ContainsAny(msg, "xin chao", "hello", "hi", "chao"))
        {
            return new
            {
                reply = "Chao ban! Toi la tro ly FastShip." +
                        "\n\nHay thu go:" +
                        "\n- 'Phi ship' de biet phi van chuyen" +
                        "\n- '#123' de tra cuu don hang" +
                        "\n- 'Goi y' de xem mon ngon",
                quickReplies = new[] { "Phi ship tinh the nao?", "Goi y mon an", "Tra cuu don hang" }
            };
        }

        return null;
    }

    private object? HandleOrderLookup(string msg)
    {
        var match = Regex.Match(msg, @"#?(\d{2,8})");
        if (!match.Success) return null;

        int orderId = int.Parse(match.Groups[1].Value);

        var donHang = db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper)
            .FirstOrDefault(d => d.madh == orderId);

        if (donHang == null)
        {
            return new
            {
                reply = "Khong tim thay don hang ma #" + orderId + ". Kiem tra lai ma don hang.",
                quickReplies = new[] { "Phi ship", "Goi y mon an", "Dang ky ban hang" }
            };
        }

        var shipperText = donHang.tbShipper != null
            ? "\n- Nguoi giao: " + donHang.tbShipper.tenshipper
            : "\n- Nguoi giao: Chua co shipper";

        return new
        {
            reply = "Don hang #" + donHang.madh +
                    "\n- Nha hang: " + donHang.tbQuanAn?.tenquanan +
                    "\n- Nguoi nhan: " + donHang.tbThongTinDatHang?.tennguoinhan +
                    "\n- Dia chi: " + donHang.tbThongTinDatHang?.diachi +
                    "\n- Tong tien: " + (donHang.tongtien?.ToString("N0") ?? "0") + " VND" +
                    "\n- Trang thai: " + donHang.trangthai +
                    shipperText +
                    "\n- Ngay dat: " + donHang.ngaydathang?.ToString("dd/MM/yyyy HH:mm"),
            quickReplies = new[] { "Phi ship", "Goi y mon an", "Dang ky ban hang" }
        };
    }

    private object? HandleRecommendation(string msg)
    {
        bool isRecommend = ContainsAny(msg, "goi y", "de xuat", "nen an", "ban chay", "hot", "ngon");
        if (!isRecommend) return null;

        var topMonAn = db.tbChiTietDonHang
            .Where(ct => ct.tbMonAn != null)
            .GroupBy(ct => new { ct.mamon, ten = ct.tbMonAn!.tenmon, gia = ct.tbMonAn!.giatien })
            .Select(g => new
            {
                ten = g.Key.ten,
                gia = g.Key.gia,
                soLuong = g.Sum(ct => ct.soluong ?? 0)
            })
            .OrderByDescending(g => g.soLuong)
            .Take(3)
            .ToList();

        if (topMonAn.Count == 0)
        {
            return new
            {
                reply = "Hien tai chua co du lieu ban chay. Hay kham pha cac quan an tren trang chu!",
                quickReplies = new[] { "Phi ship", "Dang ky ban hang", "Xem quan an" }
            };
        }

        var replyText = "Top mon an ban chay nhat:\n";
        int index = 1;
        foreach (var mon in topMonAn)
        {
            replyText += "\n" + index + ". " + mon.ten + " - " + (mon.gia?.ToString("N0") ?? "0") + "d (Da ban " + mon.soLuong + " suat)";
            index++;
        }
        replyText += "\n\nDat ngay de thuong thuc!";

        return new
        {
            reply = replyText,
            quickReplies = new[] { "Phi ship", "Dang ky ban hang", "Tra cuu don hang" }
        };
    }

    private bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var k in keywords)
            if (text.Contains(k)) return true;
        return false;
    }
}
