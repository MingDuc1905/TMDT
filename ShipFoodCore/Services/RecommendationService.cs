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

        var orderedMonIds = await _db.tbChiTietDonHang
            .Where(ct => ct.tbDonHang!.tbThongTinDatHang!.userid == userId)
            .Select(ct => ct.mamon)
            .Distinct()
            .ToListAsync();

        if (orderedMonIds.Count == 0)
            return await GetTrendingItems(take);

        var similarUserOrders = await _db.tbChiTietDonHang
            .Where(ct => ct.tbDonHang!.tbThongTinDatHang!.userid != userId
                        && orderedMonIds.Contains(ct.mamon ?? 0))
            .Select(ct => ct.mamon)
            .Distinct()
            .ToListAsync();

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
            .Include(m => m.tbBienTheMonAns)
            .ToListAsync();
    }

    /// <summary>
    /// ════════════════════════════════════════════════════════════════
    /// THUẬT TOÁN APRIORI NÂNG CAO — Support & Confidence
    /// ════════════════════════════════════════════════════════════════
    /// 
    /// Công thức:
    ///   Support(A,B→C) = count(orders chứa A,B,C) / totalOrders
    ///   Confidence(A,B→C) = count(orders chứa A,B,C) / count(orders chứa A,B)
    /// 
    /// Ngưỡng:
    ///   minSupport = 0.02 (2%)
    ///   minConfidence = 0.50 (50%)
    /// 
    /// inputMonIds: tập món đầu vào (có thể rỗng, 1 món, hoặc nhiều món)
    ///   - Rỗng → trả về Popular Pairs (cặp xuất hiện cùng nhau nhiều nhất)
    ///   - 1 món → Apriori đơn (món nào thường mua kèm món này)
    ///   - Nhiều món → Apriori đa phần tử (món nào thường mua kèm combo này)
    /// ════════════════════════════════════════════════════════════════
    /// </summary>
    public async Task<List<tbMonAn>> GetAprioriRecommendations(List<int> inputMonIds, int take = 4)
    {
        // — Trường hợp rỗng: trả về Popular Pairs —
        if (inputMonIds == null || inputMonIds.Count == 0)
            return await GetPopularPairs(take);

        // Bước 1: Lấy danh sách đơn Hoàn thành
        var completedMadhs = await _db.tbDonHang
            .Where(d => d.trangthai == "Hoàn thành")
            .Select(d => (int?)d.madh)
            .ToListAsync();
        var completedMadhList = completedMadhs.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        var totalOrders = completedMadhList.Count;
        if (totalOrders == 0) return new List<tbMonAn>();

        var inputCount = inputMonIds.Count;

        // Bước 2: Tìm orderIds chứa TẤT CẢ món đầu vào
        // Dùng GroupBy → HAVING COUNT(DISTINCT mamon) = inputCount
        var orderIdsWithAllInput = await _db.tbChiTietDonHang
            .Where(ct => inputMonIds.Contains(ct.mamon ?? 0)
                      && ct.madh != null
                      && completedMadhList.Contains(ct.madh!.Value))
            .GroupBy(ct => ct.madh)
            .Where(g => g.Select(ct => ct.mamon).Distinct().Count() == inputCount)
            .Select(g => g.Key)
            .ToListAsync();

        var orderIdsWithInputList = orderIdsWithAllInput
            .Where(x => x.HasValue).Select(x => x!.Value).ToList();
        var countOrdersWithInput = orderIdsWithInputList.Count;
        if (countOrdersWithInput == 0) return await GetPopularPairs(take);

        // Bước 3: Tìm các món B xuất hiện cùng A trong cùng đơn hàng
        var candidateGroups = await _db.tbChiTietDonHang
            .Where(ct => orderIdsWithInputList.Contains(ct.madh ?? 0)
                      && !inputMonIds.Contains(ct.mamon ?? 0)
                      && ct.mamon != null)
            .GroupBy(ct => ct.mamon)
            .Select(g => new
            {
                MonId = g.Key,
                OrderCount = g.Select(x => x.madh).Distinct().Count()
            })
            .ToListAsync();

        if (candidateGroups.Count == 0) return await GetPopularPairs(take);

        // Bước 4: Tính Support và Confidence, lọc theo ngưỡng
        const decimal minSupport = 0.02m;
        const decimal minConfidence = 0.50m;

        var filtered = candidateGroups
            .Select(c => new
            {
                MonId = c.MonId,
                OrderCount = c.OrderCount,
                Support = (decimal)c.OrderCount / totalOrders,
                Confidence = countOrdersWithInput > 0
                    ? (decimal)c.OrderCount / countOrdersWithInput
                    : 0m
            })
            .Where(r => r.Support >= minSupport && r.Confidence >= minConfidence)
            .OrderByDescending(r => r.Confidence)
            .ThenByDescending(r => r.Support)
            .Take(take)
            .ToList();

        // Fallback: nếu không đạt ngưỡng, lấy top Confidence (không lọc Support)
        if (filtered.Count == 0)
        {
            filtered = candidateGroups
                .Select(c => new
                {
                    MonId = c.MonId,
                    OrderCount = c.OrderCount,
                    Support = (decimal)c.OrderCount / totalOrders,
                    Confidence = countOrdersWithInput > 0
                        ? (decimal)c.OrderCount / countOrdersWithInput
                        : 0m
                })
                .Where(r => r.Confidence >= minConfidence)
                .OrderByDescending(r => r.Confidence)
                .ThenByDescending(r => r.Support)
                .Take(take)
                .ToList();
        }

        // Fallback 2: không lọc gì, lấy top Confidence
        if (filtered.Count == 0)
        {
            filtered = candidateGroups
                .Select(c => new
                {
                    MonId = c.MonId,
                    OrderCount = c.OrderCount,
                    Support = (decimal)c.OrderCount / totalOrders,
                    Confidence = countOrdersWithInput > 0
                        ? (decimal)c.OrderCount / countOrdersWithInput
                        : 0m
                })
                .OrderByDescending(r => r.Confidence)
                .ThenByDescending(r => r.Support)
                .Take(take)
                .ToList();
        }

        var ids = filtered.Select(r => r.MonId ?? 0).ToList();
        if (ids.Count == 0) return new List<tbMonAn>();

        return await _db.tbMonAn
            .Where(m => ids.Contains(m.mamon))
            .Include(m => m.tbQuanAn)
            .Include(m => m.tbBienTheMonAns)
            .ToListAsync();
    }

    /// <summary>
    /// Gợi ý Popular Pairs — các cặp món thường xuyên xuất hiện cùng nhau nhất
    /// (dùng khi inputMonIds rỗng, hiển thị ở trang chủ "Gợi ý Combo từ AI")
    /// </summary>
    public async Task<List<tbMonAn>> GetPopularPairs(int take = 6)
    {
        var completedMadhs = await _db.tbDonHang
            .Where(d => d.trangthai == "Hoàn thành")
            .Select(d => (int?)d.madh)
            .ToListAsync();
        var completedList = completedMadhs.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (completedList.Count == 0) return new List<tbMonAn>();

        // Lấy tất cả item pairs từ orders hoàn thành
        var orderGroups = await _db.tbChiTietDonHang
            .Where(ct => ct.madh != null && ct.mamon != null && completedList.Contains(ct.madh!.Value))
            .Select(ct => new { ct.madh, ct.mamon })
            .ToListAsync();

        // Nhóm theo đơn hàng và đếm co-occurrence
        var groupsByOrder = orderGroups
            .GroupBy(x => x.madh)
            .Where(g => g.Select(x => x.mamon).Distinct().Count() >= 2)
            .ToList();

        var pairCounts = new Dictionary<(int, int), int>();
        foreach (var group in groupsByOrder)
        {
            var items = group.Select(x => x.mamon!.Value).Distinct().OrderBy(x => x).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    var key = (items[i], items[j]);
                    pairCounts.TryGetValue(key, out var count);
                    pairCounts[key] = count + 1;
                }
            }
        }

        if (pairCounts.Count == 0) return await GetTrendingItems(take);

        // Lấy top items từ các cặp xuất hiện nhiều nhất
        var topPairs = pairCounts
            .OrderByDescending(x => x.Value)
            .Take(take * 2)
            .ToList();

        var topItemIds = topPairs
            .SelectMany(x => new[] { x.Key.Item1, x.Key.Item2 })
            .Distinct()
            .Take(take)
            .ToList();

        if (topItemIds.Count == 0) return await GetTrendingItems(take);

        return await _db.tbMonAn
            .Where(m => topItemIds.Contains(m.mamon))
            .Include(m => m.tbQuanAn)
            .Include(m => m.tbBienTheMonAns)
            .ToListAsync();
    }

    /// <summary>
    /// Phân tích Apriori cho Dashboard Nhà hàng —
    /// tìm các cặp món cùng quán có Confidence cao nhất
    /// </summary>
    public async Task<List<object>> GetRestaurantAprioriInsights(int restaurantId, int take = 5)
    {
        // Lấy tất cả món của quán này
        var monAnIds = await _db.tbMonAn
            .Where(m => m.maquanan == restaurantId)
            .Select(m => m.mamon)
            .ToListAsync();

        if (monAnIds.Count < 2) return new List<object>();

        var monAnMap = await _db.tbMonAn
            .Where(m => monAnIds.Contains(m.mamon))
            .ToDictionaryAsync(m => m.mamon, m => m.tenmon);

        // Lấy các đơn hoàn thành có chứa ít nhất 2 món của quán này
        var completedMadhs = await _db.tbDonHang
            .Where(d => d.trangthai == "Hoàn thành" && d.maquan == restaurantId)
            .Select(d => (int?)d.madh)
            .ToListAsync();
        var completedList = completedMadhs.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (completedList.Count == 0) return new List<object>();

        var orderGroups = await _db.tbChiTietDonHang
            .Where(ct => ct.madh != null && ct.mamon != null
                      && completedList.Contains(ct.madh!.Value)
                      && monAnIds.Contains(ct.mamon!.Value))
            .Select(ct => new { ct.madh, ct.mamon })
            .ToListAsync();

        var groupsByOrder = orderGroups
            .GroupBy(x => x.madh)
            .Where(g => g.Select(x => x.mamon).Distinct().Count() >= 2)
            .ToList();

        var pairCounts = new Dictionary<(int, int), int>();
        var itemTotalOrders = new Dictionary<int, int>(); // số đơn chứa từng món

        foreach (var group in groupsByOrder)
        {
            var items = group.Select(x => x.mamon!.Value).Distinct().ToList();
            foreach (var item in items)
            {
                itemTotalOrders.TryGetValue(item, out var cnt);
                itemTotalOrders[item] = cnt + 1;
            }
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    var key = (items[i], items[j]);
                    pairCounts.TryGetValue(key, out var count);
                    pairCounts[key] = count + 1;
                }
            }
        }

        // Tính Confidence và Support cho từng cặp
        var insights = pairCounts
            .Select(p => new
            {
                ItemA = p.Key.Item1,
                ItemB = p.Key.Item2,
                PairCount = p.Value,
                Support = (decimal)p.Value / completedList.Count,
                ConfidenceAtoB = itemTotalOrders.TryGetValue(p.Key.Item1, out var cntA) && cntA > 0
                    ? (decimal)p.Value / cntA : 0m,
                ConfidenceBtoA = itemTotalOrders.TryGetValue(p.Key.Item2, out var cntB) && cntB > 0
                    ? (decimal)p.Value / cntB : 0m
            })
            .OrderByDescending(r => r.ConfidenceAtoB)
            .Take(take)
            .ToList();

        return insights.Select(r => (object)new
        {
            TenMonA = monAnMap.GetValueOrDefault(r.ItemA, ""),
            TenMonB = monAnMap.GetValueOrDefault(r.ItemB, ""),
            Support = Math.Round(r.Support * 100, 1),
            ConfidenceAtoB = Math.Round(r.ConfidenceAtoB * 100, 1),
            ConfidenceBtoA = Math.Round(r.ConfidenceBtoA * 100, 1),
            PairCount = r.PairCount
        }).ToList();
    }

    /// <summary>
    /// Phân tích Apriori cho Admin Dashboard —
    /// tìm mối liên hệ kết hợp giữa các Danh mục món ăn
    /// </summary>
    public async Task<List<object>> GetCategoryAprioriInsights(int take = 5)
    {
        var danhMucs = await _db.tbDanhMuc.ToListAsync();
        var dmMap = danhMucs.ToDictionary(d => d.madanhmuc, d => d.tendanhmuc);

        var completedMadhs = await _db.tbDonHang
            .Where(d => d.trangthai == "Hoàn thành")
            .Select(d => (int?)d.madh)
            .ToListAsync();
        var completedList = completedMadhs.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (completedList.Count == 0) return new List<object>();

        // Map món → danh mục
        var monToDm = await _db.tbMonAn
            .Where(m => m.madanhmuc != null)
            .ToDictionaryAsync(m => m.mamon, m => m.madanhmuc!.Value);

        var orderGroups = await _db.tbChiTietDonHang
            .Where(ct => ct.madh != null && ct.mamon != null
                      && completedList.Contains(ct.madh!.Value)
                      && monToDm.Keys.Contains(ct.mamon!.Value))
            .Select(ct => new { ct.madh, ct.mamon })
            .ToListAsync();

        var groupsByOrder = orderGroups
            .GroupBy(x => x.madh)
            .Where(g => g.Select(x => monToDm.GetValueOrDefault(x.mamon!.Value, 0))
                          .Distinct().Count() >= 2)
            .ToList();

        var pairCounts = new Dictionary<(int, int), int>();
        var dmTotalOrders = new Dictionary<int, int>();

        foreach (var group in groupsByOrder)
        {
            var dmIds = group
                .Select(x => monToDm.GetValueOrDefault(x.mamon!.Value, 0))
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            foreach (var dmId in dmIds)
            {
                dmTotalOrders.TryGetValue(dmId, out var cnt);
                dmTotalOrders[dmId] = cnt + 1;
            }
            for (int i = 0; i < dmIds.Count; i++)
            {
                for (int j = i + 1; j < dmIds.Count; j++)
                {
                    var key = (dmIds[i], dmIds[j]);
                    pairCounts.TryGetValue(key, out var count);
                    pairCounts[key] = count + 1;
                }
            }
        }

        var insights = pairCounts
            .Select(p => new
            {
                DmIdA = p.Key.Item1,
                DmIdB = p.Key.Item2,
                PairCount = p.Value,
                Support = (decimal)p.Value / completedList.Count,
                ConfidenceAtoB = dmTotalOrders.TryGetValue(p.Key.Item1, out var cntA) && cntA > 0
                    ? (decimal)p.Value / cntA : 0m,
                ConfidenceBtoA = dmTotalOrders.TryGetValue(p.Key.Item2, out var cntB) && cntB > 0
                    ? (decimal)p.Value / cntB : 0m
            })
            .OrderByDescending(r => r.Support)
            .Take(take)
            .ToList();

        return insights.Select(r => (object)new
        {
            DanhMucA = dmMap.GetValueOrDefault(r.DmIdA, ""),
            DanhMucB = dmMap.GetValueOrDefault(r.DmIdB, ""),
            Support = Math.Round(r.Support * 100, 1),
            ConfidenceAtoB = Math.Round(r.ConfidenceAtoB * 100, 1),
            ConfidenceBtoA = Math.Round(r.ConfidenceBtoA * 100, 1),
            PairCount = r.PairCount
        }).ToList();
    }

    /// <summary>
    /// Gợi ý "Thường được mua kèm" (giữ lại vì tương thích ngược)
    /// </summary>
    public async Task<List<tbMonAn>> GetFrequentlyBoughtTogether(int? monAnId, int take = 4)
    {
        if (monAnId == null) return new List<tbMonAn>();
        return await GetAprioriRecommendations(new List<int> { monAnId.Value }, take);
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
            .Include(m => m.tbBienTheMonAns).ThenInclude(b => b.tbChiTietDonHangs)
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
