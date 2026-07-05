using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ShipFood.Hubs;
using ShipFood.Models;

namespace ShipFood.Controllers;

public class AdminChatController : BaseController
{
    private readonly IHubContext<Chats> _hubContext;

    public AdminChatController(dbFoodyEntities context, IHubContext<Chats> hubContext)
    {
        db = context;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Trang chat admin - danh sách các cuộc hội thoại
    /// </summary>
    public ActionResult Index()
    {
        if (!checkAdmin())
            return RedirectToAction("Login", "Home");

        // Lấy danh sách đơn hàng gần đây (có thể chat)
        var donHangs = db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper)
            .Include(d => d.tbChiTietDonHangs)
            .OrderByDescending(d => d.ngaydathang)
            .Take(50)
            .ToList();

        return View(donHangs);
    }

    /// <summary>
    /// API: Gửi tin nhắn từ admin đến khách hàng
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> SendMessage(int orderId, string message)
    {
        if (!checkAdmin())
            return Json(new { success = false });

        if (string.IsNullOrWhiteSpace(message))
            return Json(new { success = false, error = "Vui lòng nhập nội dung tin nhắn" });

        try
        {
            // Lưu tin nhắn vào database
            var tinNhan = new tbTinNhan
            {
                madh = orderId,
                noidung = message,
                makh = null,
                mashipper = null
            };
            db.tbTinNhans.Add(tinNhan);
            await db.SaveChangesAsync();

            // Gửi qua SignalR đến group đơn hàng (không gửi lại cho admin)
            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("adminMessage", message, orderId, "Admin");

            return Json(new { success = true });
        }
        catch
        {
            return Json(new { success = false, error = "Lỗi khi gửi tin nhắn" });
        }
    }

    /// <summary>
    /// API: Lấy lịch sử tin nhắn của một đơn hàng
    /// </summary>
    [HttpGet]
    public JsonResult GetMessages(int orderId)
    {
        if (!checkAdmin())
            return Json(new { success = false });

        var messages = db.tbTinNhans
            .Where(t => t.madh == orderId)
            .OrderBy(t => t.matn)
            .Select(t => new
            {
                id = t.matn,
                content = t.noidung,
                sender = t.mashipper != null ? "Shipper" : (t.makh != null ? "Khách hàng" : "Admin"),
                orderId = t.madh
            })
            .ToList();

        return Json(new { success = true, data = messages });
    }

    /// <summary>
    /// API: Khách hàng gửi tin nhắn (từ widget chat) — lưu DB + broadcast SignalR qua Groups
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> CustomerSendMessage(int orderId, string message)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Json(new { success = false, error = "Vui lòng đăng nhập" });

        if (string.IsNullOrWhiteSpace(message))
            return Json(new { success = false, error = "Vui lòng nhập nội dung" });

        try
        {
            // Lưu tin nhắn vào DB
            var tinNhan = new tbTinNhan
            {
                madh = orderId > 0 ? orderId : null,
                noidung = message,
                makh = user.userid,
                mashipper = null
            };
            db.tbTinNhans.Add(tinNhan);
            await db.SaveChangesAsync();

            // Broadcast đến group customer_{userId} — admin đã join group này khi chọn khách hàng
            await _hubContext.Clients.Group($"customer_{user.userid}").SendAsync("customerMessage", message, orderId, user.username, user.userid);

            // Đồng thời gửi đến group đơn hàng (nếu có) — admin trong group cũng nhận
            if (orderId > 0)
            {
                await _hubContext.Clients.Group($"order_{orderId}").SendAsync("customerMessage", message, orderId, user.username, user.userid);
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// API: L?y danh sách don hàng c?a user hi?n t?i (d? ch?n trong widget chat)
    /// </summary>
    [HttpGet]
    public JsonResult GetUserOrders()
    {
        var user = GetCurrentUser();
        if (user == null)
            return Json(new List<object>());

        var orders = db.tbDonHang
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbQuanAn)
            .Where(d => d.tbThongTinDatHang != null && d.tbThongTinDatHang.userid == user.userid)
            .OrderByDescending(d => d.ngaydathang)
            .Take(10)
            .Select(d => new
            {
                madh = d.madh,
                tenquanan = d.tbQuanAn != null ? d.tbQuanAn.tenquanan : "",
                tongtien = d.tongtien ?? 0,
                trangthai = d.trangthai ?? ""
            })
            .ToList();

        return Json(orders);
    }

    /// <summary>
    /// API: L?y danh sách cu?c h?i tho?i theo khách hàng (cho admin panel)
    /// </summary>
    [HttpGet]
    public JsonResult GetConversations()
    {
        if (!checkAdmin())
            return Json(new { success = false });

        var conversations = db.tbTinNhans
            .Where(t => t.makh != null)
            .GroupBy(t => t.makh)
            .Select(g => new
            {
                userId = g.Key,
                lastMessage = g.OrderByDescending(t => t.matn).First().noidung ?? "",
                lastTime = g.Max(t => t.matn),
                messageCount = g.Count(),
                hasUnread = g.Any(t => t.mashipper == null) // ch?a có admin tr? l?i
            })
            .OrderByDescending(x => x.lastTime)
            .Take(50)
            .ToList();

        var userIds = conversations.Select(c => c.userId).ToList();
        var users = db.tbUser.Where(u => userIds.Contains(u.userid)).ToList();
        var khachHangs = db.tbKhachHang.Where(k => userIds.Contains(k.userid)).ToList();

        var result = conversations.Select(c =>
        {
            var user = users.FirstOrDefault(u => u.userid == c.userId);
            var kh = khachHangs.FirstOrDefault(k => k.userid == c.userId);
            return new
            {
                userId = c.userId,
                tenkh = kh?.tenkh ?? user?.username ?? "Khách",
                username = user?.username ?? "",
                sdt = user?.sdt ?? "",
                lastMessage = c.lastMessage.Length > 80 ? c.lastMessage.Substring(0, 80) + "..." : c.lastMessage,
                messageCount = c.messageCount,
                hasUnread = c.hasUnread
            };
        }).ToList();

        return Json(new { success = true, data = result });
    }

    /// <summary>
    /// API: L?y tin nh?n c?a m?t khách hàng
    /// </summary>
    [HttpGet]
    public JsonResult GetCustomerMessages(int userId)
    {
        if (!checkAdmin())
            return Json(new { success = false });

        var user = db.tbUser.Find(userId);
        if (user == null)
            return Json(new { success = false, message = "Không tìm thấy người dùng" });

        var messages = db.tbTinNhans
            .Where(t => t.makh == userId)
            .OrderBy(t => t.matn)
            .Select(t => new
            {
                id = t.matn,
                content = t.noidung ?? "",
                sender = t.mashipper != null ? "Shipper" : (t.makh != null ? "Khách hàng" : "Admin"),
                orderId = t.madh
            })
            .ToList();

        return Json(new
        {
            success = true,
            data = messages,
            customerName = user.username
        });
    }

    /// <summary>
    /// API: Admin g?i tin nh?n cho khách hàng
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> SendMessageToCustomer(int userId, string message)
    {
        if (!checkAdmin())
            return Json(new { success = false });

        if (string.IsNullOrWhiteSpace(message))
            return Json(new { success = false, error = "Vui lòng nhập nội dung" });

        try
        {
            var tinNhan = new tbTinNhan
            {
                madh = null,
                noidung = message,
                makh = userId,
                mashipper = null
            };
            db.tbTinNhans.Add(tinNhan);
            await db.SaveChangesAsync();

            // Broadcast qua SignalR d?n group c?a khách hàng
            await _hubContext.Clients.Group($"customer_{userId}").SendAsync("adminMessage", message, 0, "Admin");

            return Json(new { success = true });
        }
        catch
        {
            return Json(new { success = false, error = "Lỗi khi gửi tin nhắn" });
        }
    }

    /// <summary>
    /// API: Khách hàng l?y l?ch s? tin nh?n c?a chính h? (cho widget chat)
    /// </summary>
    [HttpGet]
    public JsonResult GetMyMessages(int orderId = 0)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Json(new { success = false, error = "Vui lòng đăng nhập" });

        IQueryable<tbTinNhan> query = db.tbTinNhans.Where(t => t.makh == user.userid);

        if (orderId > 0)
            query = query.Where(t => t.madh == orderId);

        var messages = query
            .OrderBy(t => t.matn)
            .Select(t => new
            {
                id = t.matn,
                content = t.noidung ?? "",
                sender = t.mashipper != null ? "Shipper" : (t.makh != null ? "Khách hàng" : "Admin"),
                orderId = t.madh
            })
            .ToList();

        return Json(new { success = true, data = messages });
    }

    /// <summary>
    /// API: ??m s? tin nh?n ch?a ??c c?a khách hàng (cho badge thông báo)
    /// </summary>
    [HttpGet]
    public JsonResult GetUnreadCount()
    {
        var user = GetCurrentUser();
        if (user == null)
            return Json(new { count = 0 });

        var count = db.tbTinNhans
            .Count(t => t.makh == user.userid && t.mashipper == null);

        return Json(new { count });
    }

    private bool checkAdmin()
    {
        var user = GetCurrentUser();
        return user != null && user.loaitaikhoan.Equals("Admin");
    }
}
