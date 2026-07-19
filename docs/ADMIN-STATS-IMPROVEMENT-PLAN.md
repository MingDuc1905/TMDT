# Ke hoach Cai thien Thong ke Admin Dashboard

**Ngay:** 2026-07-19
**Trang thai:** Dang thuc hien
**Pham vi:** Admin Dashboard — fix sai sot du lieu + them tinh nang moi

---

## 1. Tong quan van de

Dashboard hien tai co **8/12 thong ke khong dung du lieu dung cach**. Nguyen nhan chinh:
- 6 chart khong filter theo ngay (hien thi toan bo du lieu)
- Khach hang moi khong filter ngay dang ky
- Tong so don dem ca don da huy
- 1 API bi ten sai (ExportExcel 404)

---

## 2. Cac hang muc can fix

### Phase 1: Fix sai sot nghiem trong (uu tien cao nhat)

| # | Hang muc | Mo ta | File |
|---|----------|-------|------|
| 1.1 | **ExportExcel 404** | View goi `ExportExcel` nhung controller chi co `ExportCsv` → Sua thanh `ExportCsv` | Dashboard.cshtml |
| 1.2 | **`tongSoDon` dem sai** | Dem ca don da huy vao "Tong so don" → Chi dem don "Hoàn thành" + "Đang xử lý", hoac them field "Don da huy" rieng | AdminController.cs (GetDashboardStats) |
| 1.3 | **`khachHangMoi` khong filter ngay** | Dem tat ca khach hang active → Them truong `ngaytao` vao dbUser va filter theo fromDate-toDate | AdminController.cs (GetDashboardStats) + Model |
| 1.4 | **`GetOrderStatusPie` khong filter ngay** | Dem toan bo don → Them fromDate, toDate params | AdminController.cs + Dashboard.cshtml JS |
| 1.5 | **`GetTopRestaurants` khong filter ngay** | Top 5 toan thoi gian → Them fromDate, toDate params | AdminController.cs + Dashboard.cshtml JS |
| 1.6 | **`GetTopItems` khong filter ngay** | Top mon toan thoi gian → Them fromDate, toDate params | AdminController.cs + Dashboard.cshtml JS |
| 1.7 | **`GetCategoryStats` khong filter ngay** | Doanh thu danh muc toan thoi gian → Them fromDate, toDate params | AdminController.cs + Dashboard.cshtml JS |
| 1.8 | **Null reference `ngaydathang`** | `dh.ngaydathang!.Value.Date` co the NullRef → Them null check | AdminController.cs |

### Phase 2: Cai thien UX (uu tien trung binh)

| # | Hang muc | Mo ta | File |
|---|----------|-------|------|
| 2.1 | **Hien thi so lieu da fix** | Sau khi fix so lieu dung, hien thi them "Don da huy", "Don cho xu ly" trong System Stats | Dashboard.cshtml |
| 2.2 | **Xoa du lieu khong dung** | `quanAnMoi` va `donHuy` tra ve nhung khong hien thi → Xoa khoi JSON response | AdminController.cs |
| 2.3 | **Apriori filter ngay** | Apriori insights khong filter ngay → Them param fromDate/toDate | AdminController.cs + RecommendationService |

### Phase 3: Tinh nang moi (uu tien thap)

| # | Hang muc | Mo ta | File |
|---|----------|-------|------|
| 3.1 | **Xuat CSV voi filter ngay** | ExportCsv chi xuat don "Hoàn thành" → Them filter fromDate/toDate | AdminController.cs |
| 3.2 | **Tong quan he thong chinh xac** | Hien thi them "Don cho xu ly", "Doanh thu theo trang thai" | Dashboard.cshtml |

---

## 3. Chi tiet ky thuat

### 3.1 Fix `GetDashboardStats` (AdminController.cs:578)

**Hien tai:**
```csharp
var khachHangMoi = db.tbUser
    .Count(u => u.loaitaikhoan == "Khách hàng" && u.trangthai == 1);
```

**Sua:**
```csharp
// Dem khach hang moi theo ngay dang ky
var khachHangMoi = db.tbUser
    .Count(u => u.loaitaikhoan == "Khách hàng" && u.trangthai == 1
        && u.ngaytao >= tuNgay && u.ngaytao <= denNgay);
```

**Luu y:** Can kiem tra model `tbUser` co field `ngaytao` khong. Neu khong co thi can them vao DB.

**Tong so don:**
```csharp
// Hien tai: dem tat ca
var tongSoDon = db.tbDonHang
    .Where(dh => dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay)
    .Count();

// Sua: dem chi don hoan thanh + dang xu ly (bo da huy)
var tongSoDon = db.tbDonHang
    .Where(dh => dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay
        && dh.trangthai != "Đã hủy")
    .Count();
```

### 3.2 Fix `GetOrderStatusPie` (AdminController.cs:686)

**Hien tai:** Khong filter ngay
**Sua:** Them fromDate, toDate params
```csharp
public JsonResult GetOrderStatusPie(DateTime? fromDate, DateTime? toDate)
{
    var tuNgay = fromDate ?? DateTime.Now.AddDays(-30);
    var denNgay = toDate ?? DateTime.Now;
    
    var hoanThanh = db.tbDonHang.Count(dh => dh.trangthai == "Hoàn thành"
        && dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay);
    // ... tuong tu cho cac trang thai khac
}
```

### 3.3 Fix `GetTopRestaurants` (AdminController.cs:648)

**Hien tai:** Top 5 toan thoi gian
**Sua:** Them fromDate, toDate params
```csharp
public JsonResult GetTopRestaurants(DateTime? fromDate, DateTime? toDate)
{
    var tuNgay = fromDate ?? DateTime.Now.AddDays(-30);
    var denNgay = toDate ?? DateTime.Now;
    
    var topQuan = db.tbDonHang
        .Where(dh => dh.trangthai == "Hoàn thành" && dh.tbQuanAn != null
            && dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay)
        // ... tiep tuc nhu cu
}
```

### 3.4 Fix `GetTopItems` (AdminController.cs:703)

**Hien tai:** Top mon toan thoi gian
**Sua:** Them fromDate, toDate params
```csharp
public JsonResult GetTopItems(DateTime? fromDate, DateTime? toDate)
{
    var tuNgay = fromDate ?? DateTime.Now.AddDays(-30);
    var denNgay = toDate ?? DateTime.Now;
    
    var topItems = db.tbChiTietDonHang
        .Where(ct => ct.tbDonHang != null && ct.tbDonHang.trangthai == "Hoàn thành"
            && ct.tbDonHang.ngaydathang >= tuNgay && ct.tbDonHang.ngaydathang <= denNgay
            // ... tiep tuc nhu cu
}
```

### 3.5 Fix `GetCategoryStats` (AdminController.cs:761)

**Hien tai:** Doanh thu danh muc toan thoi gian
**Sua:** Them fromDate, toDate params
```csharp
public JsonResult GetCategoryStats(DateTime? fromDate, DateTime? toDate)
{
    var tuNgay = fromDate ?? DateTime.Now.AddDays(-30);
    var denNgay = toDate ?? DateTime.Now;
    
    var stats = db.tbChiTietDonHang
        .Where(ct => ct.tbBienTheMonAn != null && ct.tbBienTheMonAn.tbMonAn != null
            && ct.tbBienTheMonAn.tbMonAn.tbDanhMuc != null
            && ct.tbDonHang != null && ct.tbDonHang.trangthai == "Hoàn thành"
            && ct.tbDonHang.ngaydathang >= tuNgay && ct.tbDonHang.ngaydathang <= denNgay)
        // ... tiep tuc nhu cu
}
```

### 3.6 Fix ExportExcel (Dashboard.cshtml:28)

**Hien tai:**
```html
<a href="@Url.Action("ExportExcel", "Admin")" class="btn btn-success btn-sm ms-2">
```

**Sua:**
```html
<a href="@Url.Action("ExportCsv", "Admin")" class="btn btn-success btn-sm ms-2">
```

### 3.7 Fix Null Reference (AdminController.cs)

**Hien tai:**
```csharp
.GroupBy(dh => dh.ngaydathang!.Value.Date)
```

**Sua:**
```csharp
.Where(dh => dh.ngaydathang != null)
.GroupBy(dh => dh.ngaydathang!.Value.Date)
```

### 3.8 Fix JS calls them fromDate, toDate (Dashboard.cshtml)

Tat ca cac AJAX call can them params fromDate, toDate:
```javascript
// Truoc:
$.getJSON('@Url.Action("GetOrderStatusPie", "Admin")', function (data) { ... });

// Sau:
$.getJSON('@Url.Action("GetOrderStatusPie", "Admin")', { fromDate: fromDate, toDate: toDate }, function (data) { ... });
```

Ap dung cho:
- `GetOrderStatusPie`
- `GetTopRestaurants` (2 lan goi: chart + table)
- `GetTopItems`
- `GetCategoryStats`

---

## 4. Database Schema Check

Can kiem tra `tbUser` co field ngay tao khong:
- `ngaytao` (DateTime?) — ngay dang ky tai khoan
- Neu khong co → can them migration

Can kiem tra `tbDonHang`:
- `ngaydathang` (DateTime?) — ngay dat hang
- `trangthai` (string) — trang thai don hang
- `tongtien` (decimal?) — tong tien

---

## 5. Thu tu thuc hien

| Buoc | Hang muc | Thoi gian du kien |
|------|----------|-------------------|
| 1 | Kiem tra DB schema (tbUser.ngaytao) | 5 phut |
| 2 | Fix ExportExcel → ExportCsv | 1 phut |
| 3 | Fix Null Reference ngaydathang | 5 phut |
| 4 | Fix GetDashboardStats (khachHangMoi + tongSoDon) | 10 phut |
| 5 | Fix GetOrderStatusPie + JS call | 5 phut |
| 6 | Fix GetTopRestaurants + JS call | 5 phut |
| 7 | Fix GetTopItems + JS call | 5 phut |
| 8 | Fix GetCategoryStats + JS call | 5 phut |
| 9 | Xoa du lieu khong dung (quanAnMoi, donHuy) | 2 phut |
| 10 | Test toan bo dashboard | 10 phut |
| 11 | Commit + push to master | 2 phut |

**Tong thoi gian du kien:** ~55 phut

---

## 6. Criteria hoan thanh

- [ ] Tat ca chart respect bo loc ngay tren UI
- [ ] Khach hang moi dem dung theo ngay dang ky
- [ ] Tong so don khong dem don da huy
- [ ] Export CSV hoat dong (khong 404)
- [ ] Khong co NullReferenceException
- [ ] Dashboard.js load dung du lieu theo filter
- [ ] Build thanh cong, khong error
- [ ] Test tren localhost voi du lieu mau
