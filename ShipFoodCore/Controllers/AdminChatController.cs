// ============================================================
// 💬 AdminChatController — Chat Admin & Khách hàng | Admin Chat
// ============================================================
// Ý nghĩa: Xử lý chat realtime giữa Admin, Shipper, Khách hàng, Quán ăn
// Chức năng: Gửi/nhận tin nhắn qua SignalR, lịch sử chat, danh sách hội thoại,
//            phân biệt role khi gửi (admin/shipper/customer/restaurant),
//            auto-lock chat sau 30 phút đơn hoàn thành, đếm unread
// KEYWORDS: chat, tin nhan, message, SignalR, conversation, hoi thoai,
//           admin chat, customer support, realtime
// ============================================================
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

    // ponytail: security fix — helper t?p trung cho generic error response gi? l?i n?i b?
    private string SafeErrorMessage(string context) => "Hệ thống đang gặp lỗi. Vui lòng thử lại sau.";

    /// <summary>
    /// Trang chat admin - danh sách các cuộc hội thoại
    /// </summary>
    public ActionResult Index()
    {
        if (!checkAdmin())
            return RedirectToAction("Login", "Home");

        try
        {
            var user = GetCurrentUser();
            ViewBag.AdminUserId = user?.userid ?? 0;

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
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminChatController>>();
            logger.LogError(ex, "AdminChat Index failed");
            return View(new List<tbDonHang>());
        }
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

            return Json(new { success = true });        } catch (Exception ex) {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminChatController>>();
            logger.LogError(ex, "SendMessage failed for order {OrderId}", orderId);
            // ponytail: security fix — không leak exception message ra client
            return Json(new { success = false, error = SafeErrorMessage("SendMessage") });
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
                sender = (t.noidung ?? "").StartsWith("[SYSTEM]") ? "Hệ thống" : (t.mashipper != null ? "Shipper" : (t.makh != null ? "Khách hàng" : "Admin")),
                orderId = t.madh
            })
            .ToList();

        return Json(new { success = true, data = messages });
    }

    /// <summary>
    /// API: G?i tin nh?n (dùng chung cho m?i role) — t? ???ng phát hi?n role
    /// L?u DB + broadcast SignalR qua Groups phù h?p v?i role
    /// targetUserId: (optional) dùng cho shipper g?i tr?c ti?p cho customer
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> CustomerSendMessage(int orderId, string message, int? targetUserId = null)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Json(new { success = false, error = "Vui lòng đăng nhập" });

        if (string.IsNullOrWhiteSpace(message))
            return Json(new { success = false, error = "Vui lòng nhập nội dung" });

        try
        {
            bool isShipper = user.loaitaikhoan.Equals("Shipper");
            bool isCustomer = user.loaitaikhoan.Equals("Khách hàng");
            bool isAdmin = user.loaitaikhoan.Equals("Admin");
            bool isRestaurant = user.loaitaikhoan.Equals("Quán ăn");

            // ponytail: security fix — customer ch? ???c dùng user.id c?a chính h?, không dùng targetUserId
            int? customerId;
            if (isCustomer)
            {
                customerId = user.userid;
            }
            else if (targetUserId.HasValue && targetUserId.Value > 0)
            {
                // Ch? cho phép Admin/Shipper g?i tin nh?n v?i targetUserId c? th?
                if (!isAdmin && !isShipper && !isRestaurant)
                    return Json(new { success = false, error = "Không có quyền gửi tin nhắn." });
                customerId = targetUserId;
            }
            else
            {
                customerId = null;
            }

            // ═══ AUTO-LOCK: Ki?m tra n?u don hàng dã hoàn thành ho?c h?y quá 30 phút ═══
            // ponytail: simplify logic — dùng ngaygiaohang ho?c ngaythanhtoan, n?u c? 2 null thì dùng current time
            if (orderId > 0)
            {
                var order = await db.tbDonHang.FindAsync(orderId);
                if (order != null)
                {
                    bool isCompleted = order.trangthai == "Hoàn thành" || order.trangthai == "Đã hủy";
                    if (isCompleted)
                    {
                        var lockTime = order.ngaygiaohang ?? order.ngaythanhtoan;
                        if (!lockTime.HasValue || (DateTime.Now - lockTime.Value).TotalMinutes > 30)
                        {
                            return Json(new { success = false, error = "Đơn hàng đã hoàn thành/hủy hơn 30 phút. Chat đã bị khóa tự động." });
                        }
                    }
                }
            }

            // L?u tin nh?n vào DB v?i role t??ng ?ng
            var tinNhan = new tbTinNhan
            {
                madh = orderId > 0 ? orderId : null,
                noidung = message,
                makh = customerId,
                mashipper = isShipper ? user.userid : null
            };
            db.tbTinNhans.Add(tinNhan);
            await db.SaveChangesAsync();

            if (isCustomer)
            {
                // Customer g?i → broadcast ??n admin + order group
                await _hubContext.Clients.Group($"customer_{user.userid}").SendAsync("customerMessage", message, orderId, user.username, user.userid);
                await _hubContext.Clients.Group("admins").SendAsync("customerMessage", message, orderId, user.username, user.userid);
                if (orderId > 0)
                    await _hubContext.Clients.Group($"order_{orderId}").SendAsync("customerMessage", message, orderId, user.username, user.userid);
            }
            else if (isShipper && customerId.HasValue)
            {
                // Shipper g?i → broadcast ??n customer + admin
                var senderName = user.username ?? "Shipper";
                await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("directMessage", message, senderName, "Shipper");
                await _hubContext.Clients.Group("admins").SendAsync("shipperMessage", message, orderId, senderName, user.userid);
                if (orderId > 0)
                    await _hubContext.Clients.Group($"order_{orderId}").SendAsync("shipperMessage", message, orderId, senderName, user.userid);
            }
            else if (isRestaurant && customerId.HasValue)
            {
                // Quán ăn g?i → broadcast ??n customer + admin
                var senderName = user.username ?? "Quán ăn";
                await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("directMessage", message, senderName, "Quán ăn");
                await _hubContext.Clients.Group("admins").SendAsync("restaurantMessage", message, orderId, senderName, user.userid);
                if (orderId > 0)
                    await _hubContext.Clients.Group($"order_{orderId}").SendAsync("orderMessage", message, orderId, senderName, "Quán ăn", "");
            }
            else if (isAdmin && customerId.HasValue)
            {
                // Admin g?i → broadcast ??n customer + order
                await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("adminMessage", message, orderId, "Admin");
                if (orderId > 0)
                    await _hubContext.Clients.Group($"order_{orderId}").SendAsync("adminMessage", message, orderId, "Admin");
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminChatController>>();
            logger.LogError(ex, "CustomerSendMessage failed");
            // ponytail: security fix — không leak exception message ra client
            return Json(new { success = false, error = SafeErrorMessage("CustomerSendMessage") });
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
    /// API: L?y danh sách cu?c h?i tho?i (cho admin panel & shipper chat)
    /// - Admin: xem t?t c? h?i tho?i
    /// - Shipper: xem h?i tho?i v?i khách hàng
    /// </summary>
    [HttpGet]
    [HttpGet]
    public JsonResult GetConversations()
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
            return Json(new { success = false, data = new object[0] });

        bool isAdmin = currentUser.loaitaikhoan.Equals("Admin");
        bool isShipper = currentUser.loaitaikhoan.Equals("Shipper");

        try
        {
            // Admin: xem t?t c? h?i tho?i; Shipper: ch? xem h?i tho?i c?a mình
            IQueryable<tbTinNhan> query = db.tbTinNhans.Where(t => t.makh != null && t.makh.HasValue);

            if (isShipper)
            {
                var shipperCustomerIds = db.tbTinNhans
                    .Where(t => t.mashipper == currentUser.userid && t.makh != null)
                    .Select(t => t.makh!.Value)
                    .Distinct()
                    .ToList();
                query = query.Where(t => shipperCustomerIds.Contains(t.makh!.Value));
            }
            else if (!isAdmin)
            {
                return Json(new { success = false, data = new object[0] });
            }

            var messageGroups = query
                .AsEnumerable()
                .GroupBy(t => t.makh!.Value)
                .Select(g => new
                {
                    userId = g.Key,
                    lastMessage = g.OrderByDescending(t => t.matn).First().noidung ?? "",
                    lastTime = g.Max(t => t.matn),
                    messageCount = g.Count(),
                    hasUnread = g.Any(t => t.mashipper == null && t.makh != null && !(t.noidung ?? "").StartsWith("[ADMIN]"))
                })
                .OrderByDescending(x => x.lastTime)
                .Take(50)
                .ToList();

            var userIds = messageGroups.Select(c => c.userId).ToList();
            var users = db.tbUser.Where(u => userIds.Contains(u.userid)).ToList();
            var khachHangs = db.tbKhachHang.Where(k => userIds.Contains(k.userid)).ToList();

            var result = messageGroups.Select(c =>
            {
                var user = users.FirstOrDefault(u => u.userid == c.userId);
                var kh = khachHangs.FirstOrDefault(k => k.userid == c.userId);
                var displayMessage = c.lastMessage;
                if (displayMessage.StartsWith("[SYSTEM]")) displayMessage = displayMessage.Substring(8).Trim();
                else if (displayMessage.StartsWith("[ADMIN]")) displayMessage = displayMessage.Substring(7);
                return new
                {
                    userId = c.userId,
                    tenkh = kh?.tenkh ?? user?.username ?? "Khách",
                    username = user?.username ?? "",
                    sdt = "***",
                    lastMessage = displayMessage.Length > 80 ? displayMessage.Substring(0, 80) + "..." : displayMessage,
                    messageCount = c.messageCount,
                    hasUnread = c.hasUnread
                };
            }).ToList();

            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminChatController>>();
            logger.LogError(ex, "GetConversations failed");
            return Json(new { success = false, data = new object[0], message = SafeErrorMessage("GetConversations") });
        }
    }

    [HttpGet]
    public JsonResult GetCustomerMessages(int userId)
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
            return Json(new { success = false, data = new object[0] });

        bool isAdminOrShipper = currentUser.loaitaikhoan.Equals("Admin") || currentUser.loaitaikhoan.Equals("Shipper");
        if (!isAdminOrShipper)
            return Json(new { success = false, data = new object[0] });

        try
        {
            var user = db.tbUser.Find(userId);
            var customerName = user?.username ?? "Khách #" + userId;

            var messages = db.tbTinNhans
                .Where(t => t.makh == userId)
                .OrderBy(t => t.matn)
                .ToList() // Load to memory before formatting strings
                .Select(t => new
                {
                    id = t.matn,
                    content = (t.noidung ?? "").StartsWith("[ADMIN]") ? t.noidung!.Substring(7) : (t.noidung ?? ""),
                    sender = (t.noidung ?? "").StartsWith("[SYSTEM]") ? "Hệ thống" : ((t.noidung ?? "").StartsWith("[ADMIN]") ? "Admin" : (t.mashipper != null ? "Shipper" : "Khách hàng")),
                    orderId = t.madh
                })
                .ToList();

            return Json(new { success = true, data = messages, customerName });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminChatController>>();
            logger.LogError(ex, "GetCustomerMessages failed for userId {UserId}", userId);
            return Json(new { success = false, data = new object[0], message = SafeErrorMessage("GetCustomerMessages") });
        }
    }

    [HttpPost]
    public async Task<JsonResult> SendMessageToCustomer(int userId, string message)
    {
        if (!checkAdmin())
            return Json(new { success = false });

        if (string.IsNullOrWhiteSpace(message))
            return Json(new { success = false, error = "Vui lòng nhập nội dung" });

        try
        {
            var latestOrder = db.tbTinNhans
                .Where(t => t.makh == userId && t.madh != null && t.madh > 0)
                .OrderByDescending(t => t.matn)
                .Select(t => t.madh)
                .FirstOrDefault();

            var tinNhan = new tbTinNhan
            {
                madh = latestOrder,
                noidung = "[ADMIN]" + message, // Gắn cờ admin để phân biệt với khách hàng
                makh = userId,
                mashipper = null
            };
            db.tbTinNhans.Add(tinNhan);
            await db.SaveChangesAsync();

            await _hubContext.Clients.Group($"customer_{userId}").SendAsync("adminMessage", message, latestOrder ?? 0, "Admin");
            if (latestOrder.HasValue && latestOrder.Value > 0)
                await _hubContext.Clients.Group($"order_{latestOrder.Value}").SendAsync("adminMessage", message, latestOrder.Value, "Admin");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminChatController>>();
            logger.LogError(ex, "SendMessageToCustomer failed for user {UserId}", userId);
            return Json(new { success = false, error = SafeErrorMessage("SendMessageToCustomer") });
        }
    }

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
            .ToList()
            .Select(t => new
            {
                id = t.matn,
                content = (t.noidung ?? "").StartsWith("[ADMIN]") ? t.noidung!.Substring(7) : (t.noidung ?? ""),
                sender = (t.noidung ?? "").StartsWith("[SYSTEM]") ? "Hệ thống" : ((t.noidung ?? "").StartsWith("[ADMIN]") ? "Admin" : (t.mashipper != null ? "Shipper" : "Khách hàng")),
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
