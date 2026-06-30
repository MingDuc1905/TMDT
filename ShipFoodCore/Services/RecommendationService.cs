using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Services;

public class RecommendationService
{
    private readonly dbFoodyEntities _db;

    public RecommendationService(dbFoodyEntities db)
    {
        _db = db;
    }

    /// <summary>
    /// Gợi ý món ăn dựa trên lịch sử đặt hàng của khách hàng (cá nhân hóa)
    /// </summary>
    public async Task<List<tbMonAn>> GetPersonalizedRecommendations(int? userId, int take = 6)
    {
        if (userId == null) return await GetTrendingItems(take);

        // Lấy danh sách món đã đặt của user
        var orderedMonIds = await _db.tbChiTietDonHang
            .Where(ct => ct.tbDonHang!.tbThongTinDatHang!.userid == userId)
            .Select(ct => ct.mamon)
            .Distinct()
            .ToListAsync();

        if (orderedMonIds.Count == 0)
            return await GetTrendingItems(take);

        // Collaborative filtering: tìm user khác có cùng sở thích
        var similarUserOrders = await _db.tbChiTietDonHang
            .Where(ct => ct.tbDonHang!.tbThongTinDatHang!.userid != userId
                        && orderedMonIds.Contains(ct.mamon ?? 0))
            .Select(ct => ct.mamon)
            .Distinct()
            .ToListAsync();

        // Gợi ý món từ user tương tự mà user chưa đặt
        var recommendIds = similarUserOrders
            .Where(id => !orderedMonIds.Contains(id))
            .Distinct()
            .Take(take)
            .ToList();

        if (recommendIds.Count == 0)
            return await GetTrendingItems(take);

        return await _db.tbMonAn
            .Where(m => recommendIds.Contains(m.mamon))
            .Include(m => m.tbQuanAn)
            .ToListAsync();
    }

    /// <summary>
    /// Gợi ý "Thường được mua kèm" - dựa trên các món xuất hiện cùng nhau trong cùng đơn hàng
    /// </summary>
    public async Task<List<tbMonAn>> GetFrequentlyBoughtTogether(int? monAnId, int take = 4)
    {
        if (monAnId == null) return new List<tbMonAn>();

        // Tìm các đơn hàng có chứa món này
        var relatedOrderIds = await _db.tbChiTietDonHang
            .Where(ct => ct.mamon == monAnId)
            .Select(ct => ct.madh)
            .Distinct()
            .ToListAsync();

        if (relatedOrderIds.Count == 0) return new List<tbMonAn>();

        // Tìm các món khác xuất hiện trong cùng đơn hàng
        var pairedMonIds = await _db.tbChiTietDonHang
            .Where(ct => relatedOrderIds.Contains(ct.madh ?? 0)
                        && ct.mamon != monAnId)
            .GroupBy(ct => ct.mamon)
            .Select(g => new { monId = g.Key, count = g.Count() })
            .OrderByDescending(g => g.count)
            .Take(take)
            .ToListAsync();

        if (pairedMonIds.Count == 0) return new List<tbMonAn>();

        var ids = pairedMonIds.Select(p => p.monId ?? 0).ToList();
        return await _db.tbMonAn
            .Where(m => ids.Contains(m.mamon))
            .Include(m => m.tbQuanAn)
            .ToListAsync();
    }

    /// <summary>
    /// Gợi ý theo xu hướng & khung giờ
    /// </summary>
    public async Task<List<tbMonAn>> GetTimeBasedRecommendations(int take = 4)
    {
        var hour = DateTime.Now.Hour;
        string[] keywords;

        if (hour >= 5 && hour < 10)
            keywords = new[] { "phở", "bún", "bánh mì", "cà phê", "sáng", "hủ tiếu", "mì" };
        else if (hour >= 11 && hour < 14)
            keywords = new[] { "cơm", "cơm văn phòng", "cơm tấm", "bún", "phở" };
        else if (hour >= 17 && hour < 22)
            keywords = new[] { "cơm", "gà", "lẩu", "nướng", "bít tết", "hải sản" };
        else
            keywords = new[] { "trà sữa", "ăn vặt", "bánh", "snack", "kem" };

        var allMonAn = await _db.tbMonAn
            .Include(m => m.tbQuanAn)
            .Include(m => m.tbChiTietDonHangs)
            .ToListAsync();

        // Ưu tiên món có tên chứa từ khóa theo khung giờ, và có số lượng bán cao
        var scored = allMonAn
            .Select(m => new
            {
                Mon = m,
                KeywordScore = keywords.Any(k => m.tenmon.ToLower().Contains(k)) ? 100 : 0,
                SalesScore = m.tbChiTietDonHangs.Sum(c => c.soluong ?? 0)
            })
            .OrderByDescending(x => x.KeywordScore + x.SalesScore)
            .Take(take)
            .Select(x => x.Mon)
            .ToList();

        return scored;
    }

    /// <summary>
    /// Top món hot trend (bán chạy trong 48h qua)
    /// </summary>
    public async Task<List<tbMonAn>> GetTrendingItems(int take = 6)
    {
        var twoDaysAgo = DateTime.Now.AddDays(-2);

        var trending = await _db.tbChiTietDonHang
            .Where(ct => ct.tbDonHang!.ngaydathang >= twoDaysAgo)
            .GroupBy(ct => ct.mamon)
            .Select(g => new { monId = g.Key, count = g.Sum(ct => ct.soluong ?? 0) })
            .OrderByDescending(g => g.count)
            .Take(take)
            .ToListAsync();

        if (trending.Count == 0)
        {
            // Fallback: top món bán chạy mọi thời đại
            trending = await _db.tbChiTietDonHang
                .GroupBy(ct => ct.mamon)
                .Select(g => new { monId = g.Key, count = g.Sum(ct => ct.soluong ?? 0) })
                .OrderByDescending(g => g.count)
                .Take(take)
                .ToListAsync();
        }

        var ids = trending.Select(t => t.monId ?? 0).ToList();
        return await _db.tbMonAn
            .Where(m => ids.Contains(m.mamon))
            .Include(m => m.tbQuanAn)
            .ToListAsync();
    }
}
