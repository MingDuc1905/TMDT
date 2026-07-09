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

        // orderedMonIds = tbBienTheMonAn.id values (ct.mamon = FK→tbBienTheMonAn.id)
        var orderedBientheIds = await _db.tbChiTietDonHang
            .Where(ct => ct.tbDonHang!.tbThongTinDatHang!.userid == userId)
            .Select(ct => ct.mamon)
            .Distinct()
            .ToListAsync();

        if (orderedBientheIds.Count == 0)
            return await GetTrendingItems(take);

        var similarUserBientheIds = await _db.tbChiTietDonHang
            .Where(ct => ct.tbDonHang!.tbThongTinDatHang!.userid != userId
                        && orderedBientheIds.Contains(ct.mamon ?? 0))
            .Select(ct => ct.mamon)
            .Distinct()
            .ToListAsync();

        var recommendBientheIds = similarUserBientheIds
            .Where(id => !orderedBientheIds.Contains(id))
            .Distinct()
            .Take(take)
            .ToList();

        if (recommendBientheIds.Count == 0)
            return await GetTrendingItems(take);

        // Chuyển tbBienTheMonAn.id → tbMonAn.mamon để query tbMonAn
        var recommendMonAnIds = await _db.tbBienTheMonAn
            .Where(b => recommendBientheIds.Contains(b.id))
            .Select(b => b.mamon)
            .Distinct()
            .ToListAsync();

        if (recommendMonAnIds.Count == 0)
            return await GetTrendingItems(take);

        return await _db.tbMonAn
            .Where(m => recommendMonAnIds.Contains(m.mamon))
            .Include(m => m.tbQuanAn)
            .Include(m => m.tbBienTheMonAns)
            .ToListAsync();
    }

    /// <summary>
    /// ════════════════════════════════════════════════════════════════
    /// THUẬT TOÁN APRIORI NÂNG CAO — Support & Confidence
    /// ════════════════════════════════════════════════════════════════
    /// 
    /// Công thức (chuẩn F&B):
    ///   Support(A→B) = count(orders chứa A∩B) / count(orders chứa A)
    ///     → Chia cho tổng đơn có chứa món A, không phải tổng toàn hệ thống
    ///     → Giữ chỉ số ổn định, không tiệm cận 0% khi số hóa đơn tăng
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

        // inputMonIds là tbMonAn.mamon, cần chuyển sang tbBienTheMonAn.id để so sánh với ct.mamon
        var inputBientheIds = await _db.tbBienTheMonAn
            .Where(b => inputMonIds.Contains(b.mamon))
            .Select(b => b.id)
            .ToListAsync();

        if (inputBientheIds.Count == 0) return await GetPopularPairs(take);
        var inputBientheCount = inputBientheIds.Count;

        // Bước 2: Tìm orderIds chứa TẤT CẢ biến thể đầu vào
        var orderIdsWithAllInput = await _db.tbChiTietDonHang
            .Where(ct => inputBientheIds.Contains(ct.mamon ?? 0)
                      && ct.madh != null
                      && completedMadhList.Contains(ct.madh!.Value))
            .GroupBy(ct => ct.madh)
            .Where(g => g.Select(ct => ct.mamon).Distinct().Count() == inputBientheCount)
            .Select(g => g.Key)
            .ToListAsync();

        var orderIdsWithInputList = orderIdsWithAllInput
            .Where(x => x.HasValue).Select(x => x!.Value).ToList();
        var countOrdersWithInput = orderIdsWithInputList.Count;
        if (countOrdersWithInput == 0) return await GetPopularPairs(take);

        // Bước 3: Tìm các biến thể B xuất hiện cùng A trong cùng đơn hàng
        var candidateGroups = await _db.tbChiTietDonHang
            .Where(ct => orderIdsWithInputList.Contains(ct.madh ?? 0)
                      && !inputBientheIds.Contains(ct.mamon ?? 0)
                      && ct.mamon != null)
            .GroupBy(ct => ct.mamon)
            .Select(g => new
            {
                BientheId = g.Key,
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
                BientheId = c.BientheId,
                OrderCount = c.OrderCount,
                Support = countOrdersWithInput > 0
                    ? (decimal)c.OrderCount / countOrdersWithInput
                    : 0m,
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
                    BientheId = c.BientheId,
                    OrderCount = c.OrderCount,
                    Support = countOrdersWithInput > 0
                        ? (decimal)c.OrderCount / countOrdersWithInput
                        : 0m,
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
                    BientheId = c.BientheId,
                    OrderCount = c.OrderCount,
                    Support = countOrdersWithInput > 0
                        ? (decimal)c.OrderCount / countOrdersWithInput
                        : 0m,
                    Confidence = countOrdersWithInput > 0
                        ? (decimal)c.OrderCount / countOrdersWithInput
                        : 0m
                })
                .OrderByDescending(r => r.Confidence)
                .ThenByDescending(r => r.Support)
                .Take(take)
                .ToList();
        }

        // Chuyển tbBienTheMonAn.id → tbMonAn.mamon
        var bientheIds = filtered.Select(r => r.BientheId ?? 0).ToList();
        if (bientheIds.Count == 0) return new List<tbMonAn>();
        var monAnIds = await _db.tbBienTheMonAn
            .Where(b => bientheIds.Contains(b.id))
            .Select(b => b.mamon)
            .Distinct()
            .ToListAsync();

        return await _db.tbMonAn
            .Where(m => monAnIds.Contains(m.mamon))
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

        // Chuyển tbBienTheMonAn.id → tbMonAn.mamon
        var monAnIds = await _db.tbBienTheMonAn
            .Where(b => topItemIds.Contains(b.id))
            .Select(b => b.mamon)
            .Distinct()
            .ToListAsync();

        if (monAnIds.Count == 0) return await GetTrendingItems(take);

        return await _db.tbMonAn
            .Where(m => monAnIds.Contains(m.mamon))
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
        // Lấy tất cả món của quán này (tbMonAn.mamon)
        var monAnIds = await _db.tbMonAn
            .Where(m => m.maquanan == restaurantId)
            .Select(m => m.mamon)
            .ToListAsync();

        if (monAnIds.Count < 2) return new List<object>();

        var monAnMap = await _db.tbMonAn
            .Where(m => monAnIds.Contains(m.mamon))
            .ToDictionaryAsync(m => m.mamon, m => m.tenmon);

        // 🐛 FIX: Chuyển tbMonAn.mamon → tbBienTheMonAn.id để so sánh với ct.mamon (FK→tbBienTheMonAn.id)
        var bientheIds = await _db.tbBienTheMonAn
            .Where(b => monAnIds.Contains(b.mamon))
            .Select(b => b.id)
            .ToListAsync();
        if (bientheIds.Count < 2) return new List<object>();

        // Lấy các đơn hoàn thành có chứa ít nhất 2 món của quán này
        var completedMadhs = await _db.tbDonHang
            .Where(d => d.trangthai == "Hoàn thành" && d.maquan == restaurantId)
            .Select(d => (int?)d.madh)
            .ToListAsync();
        var completedList = completedMadhs.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (completedList.Count == 0) return new List<object>();

        // 🐛 FIX: Dùng bientheIds thay vì monAnIds khi filter ct.mamon
        var orderGroups = await _db.tbChiTietDonHang
            .Where(ct => ct.madh != null && ct.mamon != null
                      && completedList.Contains(ct.madh!.Value)
                      && bientheIds.Contains(ct.mamon!.Value))
            .Select(ct => new { ct.madh, ct.mamon })
            .ToListAsync();

        var groupsByOrder = orderGroups
            .GroupBy(x => x.madh)
            .Where(g => g.Select(x => x.mamon).Distinct().Count() >= 2)
            .ToList();

        var pairCounts = new Dictionary<(int, int), int>();
        var itemTotalOrders = new Dictionary<int, int>();

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

        // 🐛 FIX: Chuyển tbBienTheMonAn.id → tbMonAn.mamon để tra trong monAnMap
        var bientheIdToMonAnId = await _db.tbBienTheMonAn
            .Where(b => bientheIds.Contains(b.id))
            .ToDictionaryAsync(b => b.id, b => b.mamon);

        // Tính Confidence và Support cho từng cặp
        var insights = pairCounts
            .Select(p => new
            {
                BientheIdA = p.Key.Item1,
                BientheIdB = p.Key.Item2,
                MonIdA = bientheIdToMonAnId.GetValueOrDefault(p.Key.Item1, 0),
                MonIdB = bientheIdToMonAnId.GetValueOrDefault(p.Key.Item2, 0),
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
            TenMonA = monAnMap.GetValueOrDefault(r.MonIdA, ""),
            TenMonB = monAnMap.GetValueOrDefault(r.MonIdB, ""),
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

        // 🐛 FIX: Bridge tbMonAn.mamon → tbBienTheMonAn.id để filter ct.mamon đúng FK
        var bientheToMonAn = await _db.tbBienTheMonAn
            .Where(b => monToDm.Keys.Contains(b.mamon))
            .ToDictionaryAsync(b => b.id, b => b.mamon);
        var bientheIds = bientheToMonAn.Keys.ToList();

        if (bientheIds.Count == 0) return new List<object>();

        var orderGroups = await _db.tbChiTietDonHang
            .Where(ct => ct.madh != null && ct.mamon != null
                      && completedList.Contains(ct.madh!.Value)
                      && bientheIds.Contains(ct.mamon!.Value))
            .Select(ct => new { ct.madh, ct.mamon })
            .ToListAsync();

        var groupsByOrder = orderGroups
            .GroupBy(x => x.madh)
            .Where(g =>
            {
                // Chuyển tbBienTheMonAn.id → tbMonAn.mamon → danh mục
                var monIds = g.Select(x => bientheToMonAn.GetValueOrDefault(x.mamon!.Value, 0))
                              .Where(id => id > 0)
                              .Distinct()
                              .ToList();
                var dmIds = monIds.Select(mid => monToDm.GetValueOrDefault(mid, 0))
                                  .Where(id => id > 0)
                                  .Distinct()
                                  .ToList();
                return dmIds.Count >= 2;
            })
            .ToList();

        var pairCounts = new Dictionary<(int, int), int>();
        var dmTotalOrders = new Dictionary<int, int>();

        foreach (var group in groupsByOrder)
        {
            var monIds = group
                .Select(x => bientheToMonAn.GetValueOrDefault(x.mamon!.Value, 0))
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            var dmIds = monIds.Select(mid => monToDm.GetValueOrDefault(mid, 0))
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
        // Chuyển tbBienTheMonAn.id → tbMonAn.mamon
        var monAnIds = await _db.tbBienTheMonAn
            .Where(b => ids.Contains(b.id))
            .Select(b => b.mamon)
            .Distinct()
            .ToListAsync();
        if (monAnIds.Count == 0) return new List<tbMonAn>();
        return await _db.tbMonAn
            .Where(m => monAnIds.Contains(m.mamon))
            .Include(m => m.tbQuanAn)
            .ToListAsync();
    }
}
