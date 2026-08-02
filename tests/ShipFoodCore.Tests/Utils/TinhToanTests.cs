using ShipFood.Utils;

namespace ShipFoodCore.Tests.Utils;

/// <summary>
/// Tests for TinhToan.GioVietNam — chuyển giờ UTC sang giờ Việt Nam (GMT+7).
/// Bug gốc: đơn đặt lúc 14:00 VN bị hiển thị 07:00 vì server Render chạy UTC
/// và view render thẳng ngaydathang (UTC) không đổi sang GMT+7.
/// </summary>
public class TinhToanTests
{
    [Fact]
    public void GioVietNam_Utc7am_ReturnsVn2pm()
    {
        // Đơn đặt 14:00 VN → lưu 07:00 UTC → hiển thị phải trả về 14:00 VN
        var utc = new DateTime(2026, 8, 2, 7, 0, 0, DateTimeKind.Utc);
        var result = TinhToan.GioVietNam(utc);
        Assert.NotNull(result);
        Assert.Equal(new DateTime(2026, 8, 2, 14, 0, 0), result!.Value);
    }

    [Fact]
    public void GioVietNam_UtcAfternoon_ReturnsVnEvening()
    {
        var utc = new DateTime(2026, 8, 2, 12, 30, 0);
        var result = TinhToan.GioVietNam(utc);
        Assert.Equal(new DateTime(2026, 8, 2, 19, 30, 0), result!.Value);
    }

    [Fact]
    public void GioVietNam_CrossMidnight_ShiftsDate()
    {
        // 2026-08-01 23:00 UTC = 2026-08-02 06:00 VN (sang ngày hôm sau)
        var utc = new DateTime(2026, 8, 1, 23, 0, 0);
        var result = TinhToan.GioVietNam(utc);
        Assert.Equal(new DateTime(2026, 8, 2, 6, 0, 0), result!.Value);
    }

    [Fact]
    public void GioVietNam_Null_ReturnsNull()
    {
        Assert.Null(TinhToan.GioVietNam(null));
    }
}
