# Fastship (ShipFood) — Architectural Solution Document

> **Phiên bản**: 1.1  
> **Mục đích**: Tài liệu giải pháp kiến trúc chi tiết nhằm khắc phục khuyết điểm vận hành và hoàn thiện nghiệp vụ cho 4 vai trò  
> **Phạm vi**: Thiết kế giải pháp + **Code thi công** — 5/15 giải pháp đã được implement

---

## 📋 Mục Lục

1. [Redis Distributed Session](#1-redis-distributed-session)
2. [EF Core Migrations](#2-ef-core-migrations)
3. [Rate Limiting](#3-rate-limiting)
4. [Centralized Logging](#4-centralized-logging)
5. [Customer — QR Payment & Webhook](#5-customer--qr-payment--webhook)
6. [Customer — Address Book](#6-customer--address-book)
7. [Customer — Hủy đơn & Hoàn tiền](#7-customer--hủy-đơn--hoàn-tiền)
8. [Restaurant — Preparing Status](#8-restaurant--preparing-status)
9. [Restaurant — Hết hàng nhanh](#9-restaurant--hết-hàng-nhanh)
10. [Restaurant — Giờ hoạt động tự động](#10-restaurant--giờ-hoạt-động-tự-động)
11. [Shipper — Auto-Matching](#11-shipper--auto-matching)
12. [Shipper — Rút tiền](#12-shipper--rút-tiền)
13. [Shipper — Proof of Delivery](#13-shipper--proof-of-delivery)
14. [Admin — Duyệt đối tác](#14-admin--duyệt-đối-tác)
15. [Admin — Đối soát & Chiết khấu](#15-admin--đối-soát--chiết-khấu)

---

## 1. Redis Distributed Session

### 1.1 Vấn đề hiện tại

```
Current Architecture (In-Memory):
┌──────────────────────────────────────────────────┐
│  Railway Load Balancer                           │
│         │            │                           │
│         ▼            ▼                           │
│  ┌──────────┐  ┌──────────┐                      │
│  │ Replica 1 │  │ Replica 2 │   ← Session mất    │
│  │ Memory   │  │ Memory   │     khi request      │
│  │ Cache A  │  │ Cache B  │     chuyển replica   │
│  └──────────┘  └──────────┘                      │
│         ↑                                          │
│   User request luân chuyển                         │
│   → Session không đồng bộ giữa 2 replica           │
└──────────────────────────────────────────────────┘
```

**Chi tiết code hiện tại** (`Program.cs`):

| Service | Implementation | Vấn đề |
|---------|---------------|--------|
| `IDistributedCache` | `AddDistributedMemoryCache()` | In-memory, không shared |
| `Session` | `AddSession()` với `IdleTimeout = 1 ngày` | Cookie HttpOnly, nhưng data lưu trong memory từng instance |
| `Get/Set Session` | `HttpContext.Session.GetString/SetString` (JSON) | Compatible với Redis |

### 1.2 Giải pháp: Redis-backed Distributed Cache

```
Target Architecture (Redis Distributed):
┌──────────────────────────────────────────────────┐
│  Railway Load Balancer                           │
│         │            │                           │
│         ▼            ▼                           │
│  ┌──────────┐  ┌──────────┐                      │
│  │ Replica 1 │  │ Replica 2 │   ← Session        │
│  │ Redis    │  │ Redis    │     đồng bộ qua      │
│  │ Client   │  │ Client   │     Redis Central    │
│  └────┬─────┘  └────┬─────┘                      │
│       └──────┬──────┘                            │
│              ▼                                    │
│    ┌──────────────────┐                          │
│    │  Redis Central   │ ← Railway Redis Add-on   │
│    │  (StackExchange) │   1 instance, shared      │
│    └──────────────────┘                          │
└──────────────────────────────────────────────────┘
```

### 1.3 Kiến trúc chi tiết

#### Package cần thêm

| Package | Version | Mục đích |
|---------|---------|----------|
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 8.0.x | Redis distributed cache provider |
| (built-in) `StackExchange.Redis` | 2.x | Dependency transitive, connection multiplexer |

#### Cấu hình Connection String (Railway Environment)

```env
# Railway Redis Plugin tự động inject
REDIS_URL=redis://default:password@redis.railway.internal:6379

# Hoặc dạng connection string full
Redis__Configuration=redis.railway.internal:6379,password=xxx,abortConnect=false,ssl=false
```

#### Cấu hình Program.cs (thay thế)

**Trước (In-Memory):**
```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

**Sau (Redis Distributed):**
```csharp
// --- Redis Distributed Cache ---
var redisConnection = builder.Configuration["Redis:Configuration"]
    ?? Environment.GetEnvironmentVariable("REDIS_URL");
    
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "FastShip_Session:";
    });
}
else
{
    // Fallback: in-memory (development)
    builder.Services.AddDistributedMemoryCache();
}

// --- Session (giữ nguyên cấu hình thời gian, nhưng data đã distributed) ---
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

#### Middleware Order (giữ nguyên, chỉ cần đảm bảo)

```csharp
app.UseRouting();
app.UseSession();     // Sau UseRouting, trước MapControllerRoute
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(...);
```

### 1.4 Serialization (ISession Extension Methods)

Code hiện tại dùng `HttpContext.Session.GetString/SetString` để lưu JSON — **hoàn toàn tương thích với Redis**. Không cần thay đổi gì ở tầng Controller/Service.

```
Session Key          → Redis Key
─────────────────────────────────────
"user"               → FastShip_Session:session_{sessionId}_user
"Cart"               → FastShip_Session:session_{sessionId}_Cart
```

`InstanceName = "FastShip_Session:"` là prefix giúp:
- Phân biệt với các app khác dùng chung Redis instance
- Dễ dàng xoá toàn bộ session keys khi cần maintenance
- Redis Insight monitor dễ đọc

### 1.5 Graceful Degradation (Redis Down)

```
Flow khi Redis không khả dụng:
┌──────────────────────────────────────────────┐
│  Request đến → Try Redis Get                 │
│         │                                    │
│         ├── Redis OK → Return session        │
│         │                                    │
│         └── Redis FAIL →                     │
│              ├── Log warning (Serilog)       │
│              └── Tạo session mới (trống)     │
│                  → User phải đăng nhập lại   │
│                  → Không crash app           │
└──────────────────────────────────────────────┘
```

**Giải pháp**: `abortConnect=false` trong connection string — `StackExchange.Redis` sẽ tự động retry kết nối background mà không crash app.

### 1.6 Railway Redis Plugin Setup

| Bước | Thao tác | Ghi chú |
|------|----------|---------|
| 1 | Railway Dashboard → New Plugin → Redis | Tạo instance |
| 2 | Copy `REDIS_URL` từ plugin | Dạng `redis://...` |
| 3 | Inject vào app service → Variable → `Redis:Configuration` | Tên biến tuỳ chỉnh |
| 4 | Redeploy → Kiểm tra `/health` | App không crash nếu Redis chưa ready |

### 1.7 Ma trận so sánh

| Tiêu chí | In-Memory (hiện tại) | Redis (giải pháp) |
|----------|---------------------|-------------------|
| Multi-replica | ❌ Không hoạt động | ✅ Đồng bộ hoàn toàn |
| Khởi động lại app | ❌ Mất session | ✅ Còn session |
| Hiệu năng | ⚡ Rất nhanh | ⚡ Nhanh (network round-trip ~1ms) |
| Chi phí | Miễn phí | Railway Redis ~$5/tháng |
| Độ phức tạp | Thấp | Trung bình (thêm 1 package) |
| Monitoring | Không | Redis Insight Dashboard |
| Fallback | Không cần | Graceful degradation |

### 1.8 Kết luận

Giải pháp này yêu cầu:
- Thêm **1 NuGet package**
- Thêm **~15 dòng code** cấu hình trong `Program.cs`
- Thêm **1 Redis plugin** trên Railway
- **Không thay đổi** bất kỳ Controller, Service, Model hay View nào

→ Đây là thay đổi kiến trúc có **impact thấp nhất** nhưng mang lại lợi ích lớn nhất cho khả năng scale.

### 1.9 ✅ Tình trạng thi công

**ĐÃ IMPLEMENT** (Tháng 7/2026):
- `ShipFoodCore.csproj`: Thêm package `Microsoft.Extensions.Caching.StackExchangeRedis` 8.0.11
- `Program.cs`: Redis config với fallback in-memory, dùng `REDIS_URL` env var
- **Trạng thái**: ✅ Đã code — cần thêm Redis plugin trên Railway để active

---

## 2. EF Core Migrations

### 2.1 Vấn đề hiện tại

```
Current State:
┌──────────────────────────────────────────────────┐
│  Program.cs: db.Database.EnsureCreated()         │
│         │                                        │
│         ▼                                        │
│  Database có bảng + dữ liệu                      │
│  Nhưng KHÔNG có bảng __EFMigrationsHistory       │
│         │                                        │
│         ▼                                        │
│  Khi Model thay đổi (thêm cột):                  │
│  → EnsureCreated() không cập nhật gì             │
│  → Phải xoá DB và tạo lại → MẤT DỮ LIỆU         │
│  → Không thể rollback                            │
│  → Không support CI/CD pipeline                  │
└──────────────────────────────────────────────────┘
```

**Bảng `__EFMigrationsHistory`** là bảng metadata mà EF Core Migrations dùng để theo dõi migration nào đã được áp dụng. `EnsureCreated()` bỏ qua bảng này hoàn toàn.

### 2.2 Giải pháp: Chuyển từ EnsureCreated → Migrations

```
Migration Strategy (Zero Data Loss):
┌──────────────────────────────────────────────────┐
│  BƯỚC 1: Backup database                        │
│  BƯỚC 2: Xoá EnsureCreated() khỏi Program.cs    │
│  BƯỚC 3: dotnet ef migrations add InitialCreate │
│         │                                        │
│         ▼                                        │
│  BƯỚC 4: Generate SQL script                     │
│          dotnet ef migrations script --output sql │
│         │                                        │
│         ▼                                        │
│  BƯỚC 5: Chỉ chạy INSERT vào __EFMigrationsHistory│
│          → EF Core tin rằng migration đã áp dụng  │
│         │                                        │
│         ▼                                        │
│  BƯỚC 6: db.Database.MigrateAsync() thay thế    │
│          EnsureCreated()                         │
│         │                                        │
│         ▼                                        │
│  Từ nay có thể thêm migration mới bình thường    │
│  dotnet ef migrations add AddNewColumn           │
│  dotnet ef database update                       │
└──────────────────────────────────────────────────┘
```

### 2.3 Quy trình chi tiết từng bước

#### Bước 1: Backup database

```sql
-- MySQL dump toàn bộ database
mysqldump -u root -p dbFoody > dbFoody_backup_$(date +%Y%m%d).sql
```

#### Bước 2: Xoá EnsureCreated() khỏi Program.cs

Xoá hoặc comment block code sau trong `Program.cs`:

```csharp
// ❌ XOÁ: var created = db.Database.EnsureCreated();
// ❌ XOÁ: if (created && !db.tbUsers.Any()) { ... seed ... }
// ❌ XOÁ: try { db.Database.ExecuteSqlRaw("ALTER TABLE...") } catch { }
```

**Giữ lại** logic seed data, nhưng chạy sau `MigrateAsync()`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<dbFoodyEntities>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // THAY THẾ: MigrateAsync() thay vì EnsureCreated()
    await db.Database.MigrateAsync();

    // Seed data nếu DB trống
    if (!db.tbUsers.Any())
    {
        // ... seed logic ...
    }

    // BCrypt fix (giữ nguyên)
    try { db.Database.ExecuteSqlRaw("ALTER TABLE tbUser MODIFY COLUMN pwd VARCHAR(255) NOT NULL;"); } catch { }
}
```

#### Bước 3: Tạo Initial Migration

```bash
# Cài tool nếu chưa có
dotnet tool install --global dotnet-ef

# Tạo migration đầu tiên
cd ShipFoodCore
dotnet ef migrations add InitialCreate
```

⚠️ **Lưu ý**: Lệnh này sẽ chạy trên **local development database** (trống). Migration file tạo ra sẽ chứa tất cả các bảng hiện có.

#### Bước 4: Sinh SQL script

```bash
dotnet ef migrations script --output initial_migration.sql
```

#### Bước 5: Fake-apply migration lên production

Mở file `initial_migration.sql`, tìm dòng `INSERT INTO __EFMigrationsHistory` ở cuối file, và chỉ chạy dòng đó trên production:

```sql
-- Chỉ chạy dòng này trên production database
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260701000000_InitialCreate', '8.0.11');
```

#### Bước 6: Về sau — Migration bình thường

```bash
# Khi thay đổi Model
dotnet ef migrations add AddDaDuyetColumn
dotnet ef database update

# Hoặc sinh script idempotent cho DBA review
dotnet ef migrations script --idempotent

# Hoặc bundle cho CI/CD
dotnet ef migrations bundle --self-contained -r linux-x64
```

### 2.4 Migration Bundles cho CI/CD (Production)

```
CI/CD Pipeline:
┌──────────────────────────────────────────────┐
│  Build: dotnet ef migrations bundle           │
│         → efbundle.exe (self-contained)       │
│         │                                     │
│         ▼                                     │
│  Deploy: copy efbundle.exe → server           │
│         │                                     │
│         ▼                                     │
│  Run: ./efbundle --connection "..."
│       → Idempotent, safe to re-run            │
│       → Dùng migration lock (EF 8+)           │
│       → An toàn với multi-replica             │
└──────────────────────────────────────────────┘
```

### 2.5 Ma trận so sánh

| Tiêu chí | EnsureCreated() (hiện tại) | Migrations (giải pháp) |
|----------|---------------------------|----------------------|
| Tạo bảng mới | ✅ Tự động | ✅ Tự động |
| Thêm cột mới | ❌ Phải xoá DB | ✅ migration add |
| Xoá cột | ❌ Không | ✅ migration add |
| Rollback | ❌ Không | ✅ `database update -1` |
| CI/CD support | ❌ Không | ✅ Bundle / Idempotent SQL |
| Dữ liệu an toàn | ❌ Dễ mất | ✅ Giữ nguyên |
| Production safety | ❌ Rủi ro cao | ✅ An toàn |

### 2.7 ✅ Tình trạng thi công

**ĐÃ IMPLEMENT** (Tháng 7/2026):
- `Program.cs`: Hybrid approach — thử `MigrateAsync()` nếu có bảng `__EFMigrationsHistory`, fallback `EnsureCreated()`
- Giữ nguyên seed + BCrypt fix
- **Trạng thái**: ✅ Đã code — cần chạy `dotnet ef migrations add InitialCreate` để tạo migration file

### 2.8 Seed Data Migration

Thay vì chạy SQL file raw, nên chuyển seed data thành Migration:

```csharp
// Trong migration file, dùng migrationBuilder.Sql()
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Thay vì chạy seed_mysql.sql ở runtime
    // Tạo migration riêng cho seed data
    migrationBuilder.Sql(@"
        INSERT IGNORE INTO tbDanhMuc (tendanhmuc) VALUES
        ('Cơm'), ('Phở'), ('Bún'), ('Lẩu'), ('Trà sữa');
    ");
}
```

Hoặc giữ nguyên pattern hiện tại (seed trong `Program.cs`) — vẫn hoạt động được, chỉ cần chạy sau `MigrateAsync()`.

---

## 3. Rate Limiting

### 3.1 Vấn đề hiện tại

| Nguy cơ | Mô tả | Impact |
|---------|-------|--------|
| **Gemini API spam** | Không giới hạn → 1 user có thể gọi chatbot 1000 lần/phút | Tốn chi phí Gemini API (pay-per-token) |
| **DDoS endpoint** | Endpoint `/Chatbot/SendMessage` không được bảo vệ | Server quá tải |
| **Abuse payment** | Endpoint `/Payment/ProcessPayment` có thể bị gọi spam | Sai lệch dữ liệu |
| **Brute-force login** | `/Home/Login` không giới hạn số lần thử | Dễ bị tấn công dictionary |

### 3.2 Giải pháp: ASP.NET Core 8 Built-in Rate Limiting

.NET 8+ tích hợp sẵn middleware rate limiting với 4 thuật toán:

| Algorithm | Mô tả | Dùng cho |
|-----------|-------|----------|
| **Fixed Window** | Giới hạn số request trong 1 khung thời gian cố định (vd: 5 request/phút) | Gemini API, Chatbot |
| **Sliding Window** | Tương tự nhưng reset mượt hơn, chia window thành segments | Payment endpoint |
| **Token Bucket** | Bucket chứa token, mỗi request tiêu tốn 1 token, refill định kỳ | Login, Signup |
| **Concurrency** | Giới hạn số request đồng thời (không theo thời gian) | Heavy operations |

### 3.3 Kiến trúc chi tiết

#### Định nghĩa Policies (Program.cs)

```csharp
// Đăng ký Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Policy 1: Gemini Chatbot — 5 request/phút/user
    options.AddFixedWindowLimiter("gemini-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0; // Từ chối ngay khi quá limit
    });

    // Policy 2: Payment — 3 request/phút
    options.AddFixedWindowLimiter("payment-policy", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Policy 3: Login — 5 lần thử trong 5 phút
    options.AddSlidingWindowLimiter("login-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.SegmentsPerWindow = 5;
        opt.QueueLimit = 0;
    });

    // Policy 4: General API — 100 request/phút
    options.AddFixedWindowLimiter("general-api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Middleware (sau UseRouting)
app.UseRateLimiter();
```

#### Áp dụng lên Controller/Action

```csharp
// ChatbotController.cs — 5 request/phút
[EnableRateLimiting("gemini-policy")]
public class ChatbotController : Controller
{
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageModel model)
    {
        // ...
    }
}

// PaymentController.cs — 3 request/phút
[EnableRateLimiting("payment-policy")]
public class PaymentController : Controller
{
    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentModel model)
    {
        // ...
    }
}

// HomeController.cs — chỉ login bị giới hạn
public class HomeController : Controller
{
    [EnableRateLimiting("login-policy")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        // ...
    }
}
```

### 3.4 Xử lý khi bị từ chối (429)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";

        // Tính thời gian retry-after
        var retryAfter = context.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryAfterDuration)
            ? retryAfterDuration.TotalSeconds.ToString()
            : "60";

        context.HttpContext.Response.Headers["Retry-After"] = retryAfter;

        await context.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(new
            {
                success = false,
                message = $"Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau {retryAfter} giây.",
                retryAfterSeconds = retryAfter
            }));
    };
});
```

### 3.5 Frontend xử lý 429

```javascript
// Trong chatbot.js hoặc _ChatWidget.cshtml
async function sendChatMessage(message) {
    try {
        const response = await fetch('/Chatbot/SendMessage', { ... });
        if (response.status === 429) {
            const data = await response.json();
            showToast('⏳ ' + data.message);
            return;
        }
        // ... xử lý bình thường
    } catch (err) {
        console.error('Chat error:', err);
    }
}
```

### 3.6 ✅ Tình trạng thi công

**ĐÃ IMPLEMENT** (Tháng 7/2026):
- `Program.cs`: 3 policies (gemini-policy 5/phút, login-policy 5/5phút, general-api 100/phút)
- `ChatbotController.cs`: `[EnableRateLimiting("gemini-policy")]`
- `HomeController.cs`: `[EnableRateLimiting("login-policy")]` trên Login POST
- Custom 429 JSON response với Retry-After + thông báo tiếng Việt
- **Trạng thái**: ✅ Đã code + apply controller attributes

### 3.7 Ma trận so sánh

| Endpoint | Policy | Limit | Window | Queue |
|----------|--------|-------|--------|-------|
| `/Chatbot/SendMessage` | gemini-policy | 5 requests | 1 phút | 0 |
| `/Payment/ProcessPayment` | payment-policy | 3 requests | 1 phút | 0 |
| `/Home/Login` | login-policy | 5 attempts | 5 phút | 0 |
| `/Home/Signup` | login-policy | 3 attempts | 1 giờ | 0 |
| `/Cart/CheckCoupon` | general-api | 20 requests | 1 phút | 0 |
| Tất cả API còn lại | general-api | 100 requests | 1 phút | 2 |

---

## 4. Centralized Logging

### 4.1 Vấn đề hiện tại

```
Current Logging:
┌──────────────────────────────────────────────────┐
│  Console.WriteLine($\"[WARN] ...\")               │
│  logger.LogInformation() — không có sink         │
│         │                                        │
│         ▼                                        │
│  Log chỉ in ra console                           │
│  → Mất log khi container restart                 │
│  → Không search được                             │
│  → Không có structured data                      │
│  → Không alert khi có lỗi                        │
└──────────────────────────────────────────────────┘
```

### 4.2 Giải pháp: Serilog + Seq

```
Target Architecture:
┌──────────────────────────────────────────────────┐
│  Ứng dụng ShipFood                               │
│         │                                        │
│         ├── Serilog (ILogger<T>)                 │
│         │    ├── Sink: Console (dev)             │
│         │    ├── Sink: Seq (production)          │
│         │    └── Enrichers: CorrelationId, ...   │
│         │                                        │
│         ▼                                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
│  │ Console  │  │   Seq    │  │ File     │       │
│  │ (stdout) │  │ (Docker) │  │ (backup) │       │
│  └──────────┘  └──────────┘  └──────────┘       │
│                      │                           │
│                      ▼                           │
│           ┌──────────────────┐                   │
│           │  Seq Dashboard   │ ← Railway add-on  │
│           │  port 5341       │   hoặc Docker     │
│           └──────────────────┘                   │
└──────────────────────────────────────────────────┘
```

### 4.3 Chi tiết triển khai

#### Packages cần thêm

| Package | Mục đích |
|---------|----------|
| `Serilog.AspNetCore` | Tích hợp Serilog với ASP.NET Core 8 |
| `Serilog.Sinks.Seq` | Gửi log đến Seq server |
| `Serilog.Sinks.Console` | In log ra console (stdout) |
| `Serilog.Enrichers.Environment` | Enrich với machine name |
| `Serilog.Enrichers.Process` | Enrich với process ID |

#### Cấu hình Program.cs (Two-stage Bootstrap)

```csharp
using Serilog;

// STAGE 1: Bootstrap Logger — bắt lỗi startup
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // STAGE 2: Serilog chính thức với config
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console();

        // Seq sink (nếu có cấu hình)
        var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
        if (!string.IsNullOrEmpty(seqUrl))
        {
            configuration.WriteTo.Seq(seqUrl, apiKey: Environment.GetEnvironmentVariable("SEQ_API_KEY"));
        }
    });

    // ... phần còn lại của builder ...

    var app = builder.Build();

    // Middleware: request logging (tự động log method, path, status, duration)
    app.UseSerilogRequestLogging();

    // ... middleware khác ...

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

#### Cấu hình appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### 4.4 Correlation ID Middleware

Thêm middleware để gắn Correlation ID vào mỗi request — giúp trace 1 request xuyên suốt các service:

```csharp
// Middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"]
        .FirstOrDefault() ?? Guid.NewGuid().ToString();

    context.Response.Headers["X-Correlation-ID"] = correlationId;

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});
```

### 4.5 Logging Patterns cho từng tầng

```
Log Level Usage Guide:
┌──────────────────────────────────────────────────┐
│  Verbose:   Debug chi tiết (không bật production)│
│  Debug:     Query SQL, dữ liệu request/response  │
│  Information:  User login, order created, payment │
│  Warning:   Redis down (fallback active)          │
│  Error:     Exception, payment fail, DB error     │
│  Fatal:     App crash, không start được           │
└──────────────────────────────────────────────────┘
```

**Các log quan trọng cần thêm:**

| Sự kiện | Level | Message mẫu | Properties |
|---------|-------|-------------|------------|
| User đăng nhập | Info | `User {UserId} logged in` | UserId, Role |
| Đơn hàng tạo | Info | `Order {OrderId} created by {UserId}` | OrderId, Total |
| Thanh toán success | Info | `Payment {PaymentId} completed for Order {OrderId}` | Amount, Method |
| Thanh toán fail | Error | `Payment failed for Order {OrderId}: {Reason}` | Stack trace |
| Gemini API call | Debug | `Gemini API call: {MessageLength} chars` | Tokens used |
| Redis disconnect | Warning | `Redis connection lost, falling back to in-memory` | — |
| Migration applied | Info | `Migration {MigrationId} applied successfully` | — |
| Rate limit hit | Warning | `Rate limit exceeded for endpoint {Path} by user {UserId}` | IP, Policy |

### 4.6 Seq Dashboard (Railway)

```
Seq Deployment Options:
┌──────────────────────────────────────────────────┐
│  Option 1: Railway Plugin                        │
│  → Seq add-on trên Railway                        │
│  → URL: https://{app}.railway.app:5341           │
│  → Tự động scale                                  │
│                                                  │
│  Option 2: Docker (tự host)                      │
│  docker run -d --name seq -p 5341:80              │
│    datalust/seq:latest                           │
│                                                  │
│  Option 3: Seq Cloud (datalust.co)               │
│  → Dùng thử 7 ngày, sau đó $10/tháng             │
└──────────────────────────────────────────────────┘
```

### 4.7 Anti-patterns cần tránh

| Anti-pattern | Sai | Đúng |
|-------------|-----|------|
| String interpolation | `Log.Info($"Order {id}")` | `Log.Info("Order {OrderId}", id)` |
| Log synchronous | Mặc định | Dùng Serilog async sink wrapper |
| Over-log request body | Log toàn bộ JSON | Chỉ log metadata |
| Không có Correlation ID | Không trace được | Mỗi request có 1 ID duy nhất |

---

## 5. Customer — QR Payment & Webhook

### 5.1 Vấn đề hiện tại

```
Current Payment Flow:
┌──────────────────────────────────────────────────┐
│  User chọn "Chuyển khoản" → Xem thông tin       │
│  tài khoản ngân hàng (text)                      │
│         │                                        │
│         ▼                                        │
│  User tự mở app bank → nhập số tài khoản         │
│  → nhập số tiền → nhập nội dung                  │
│         │                                        │
│         ▼                                        │
│  User quay lại → bấm "Đã chuyển khoản"           │
│  → Admin kiểm tra thủ công                       │
│  → Không tự động → Đơn mãi ở trạng thái chờ      │
└──────────────────────────────────────────────────┘
```

### 5.2 Giải pháp: QR Code động + Mock Webhook/IPN + SignalR

```
Target Payment Flow:
┌──────────────────────────────────────────────────┐
│  User chọn "Chuyển khoản"                       │
│         │                                        │
│         ▼                                        │
│  Hệ thống sinh mã QR động:                       │
│  - Số tài khoản cố định                          │
│  - Số tiền = tổng đơn                            │
│  - Nội dung = Mã đơn hàng (madh)                 │
│         │                                        │
│         ▼                                        │
│  User mở app bank → quét mã QR                   │
│  → Tự động điền số tiền + nội dung               │
│  → Xác nhận chuyển khoản                         │
│         │                                        │
│         ▼                                        │
│  Admin Dashboard → Nút [✅ Xác nhận thanh toán]  │
│  → Bấm vào → Gọi Mock Webhook                    │
│         │                                        │
│         ▼                                        │
│  SignalR broadcast đến user:                     │
│  "Đơn hàng #123 đã thanh toán thành công!"       │
│  → Trạng thái đơn → "Đã thanh toán"              │
└──────────────────────────────────────────────────┘
```

### 5.3 Kiến trúc QR Code

#### QR Payload Format (VietQR/EMVCo)

Sử dụng **QRCoder** NuGet package để sinh QR code dạng Base64 (không cần lưu file).

| Package | Mục đích |
|---------|----------|
| `QRCoder` | Sinh QR code trong memory, cross-platform |

#### Cấu trúc QR Payload (VietQR Standard)

```
Tag 00: Payload Format Indicator  → "01"
Tag 01: Point of Initiation Method → "11" (dynamic)
Tag 38: Merchant Account Info
  Sub 00: GUID = "A000000727" (Napas)
  Sub 01: BIN = "9704XX" (mã ngân hàng)
  Sub 02: Số tài khoản = "1903XXXXXXX"
Tag 53: Currency = "704" (VND)
Tag 54: Amount = "150000" (số tiền, không dấu phẩy)
Tag 58: Country Code = "VN"
Tag 62: Additional Data
  Sub 01: Nội dung CK = "FASTSHIP123" (mã đơn)
Tag 63: CRC (tự động tính CRC-16)
```

#### Service sinh QR (luồng xử lý)

```
Checkout → PaymentController
    │
    ▼
Generate QR:
  1. Lấy thông tin đơn hàng (madh, tongtien)
  2. Build VietQR payload string
  3. QRCoder → Base64 image
  4. Trả về View(QR_Base64, madh, tongtien)

View Checkout:
  <img src="data:image/png;base64,@Model.QR_Base64" />
  <p>Số tiền: @Model.TongTien.ToString("N0")đ</p>
  <p>Nội dung: FASTSHIP@Model.MaDH</p>
  <div id="payment-status">⏳ Chờ thanh toán...</div>
  
  <!-- SignalR lắng nghe sự kiện thanh toán -->
```

### 5.4 Mock Webhook/IPN

#### Bảng giả lập thanh toán

| Bảng | Mô tả |
|------|-------|
| `tbGiaoDichThanhToan` | Lưu lịch sử giao dịch thanh toán |

```sql
CREATE TABLE tbGiaoDichThanhToan (
    magiaodich    INT AUTO_INCREMENT PRIMARY KEY,
    madh          INT NOT NULL,
    phuongthuc    VARCHAR(20) NOT NULL DEFAULT 'ChuyenKhoan',
    soTien        DECIMAL(18,0) NOT NULL,
    noidung       VARCHAR(200),
    trangthai     VARCHAR(20) NOT NULL DEFAULT 'ChoThanhToan',
      -- 'ChoThanhToan', 'DaThanhToan', 'HoanTien'
    ngaytao       DATETIME DEFAULT CURRENT_TIMESTAMP,
    ngayxacnhan   DATETIME,
    nguoixacnhan  INT,  -- admin id
    FOREIGN KEY (madh) REFERENCES tbDonHang(madh)
);
```

#### Admin Webhook Button (Mock)

```csharp
// AdminController.cs — Mock Webhook
[HttpPost]
public async Task<IActionResult> MockPaymentWebhook(int madh)
{
    // 1. Cập nhật trạng thái đơn hàng
    var donHang = await _db.tbDonHang.FindAsync(madh);
    donHang.trangthai = "Đã thanh toán";
    
    // 2. Ghi nhận giao dịch
    var gd = new tbGiaoDichThanhToan
    {
        madh = madh,
        soTien = donHang.tongtien ?? 0,
        trangthai = "DaThanhToan",
        ngayxacnhan = DateTime.Now,
        nguoixacnhan = adminId
    };
    _db.tbGiaoDichThanhToans.Add(gd);
    await _db.SaveChangesAsync();
    
    // 3. SignalR broadcast đến user
    await _hubContext.Clients
        .Group($"order_{madh}")
        .SendAsync("paymentConfirmed", madh, donHang.tongtien);
    
    return Json(new { success = true });
}
```

#### SignalR Event (phía client)

```javascript
// Trong Checkout.cshtml / ChiTietDonHang.cshtml
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/nhantin")
    .withAutomaticReconnect()
    .build();

connection.on("paymentConfirmed", function(orderId, amount) {
    document.getElementById("payment-status").innerHTML = 
        "✅ Thanh toán thành công! Số tiền: " + 
        amount.toLocaleString('vi-VN') + "đ";
    document.getElementById("payment-status").className = "badge bg-success";
});

connection.start().then(function() {
    connection.invoke("JoinOrderGroup", @madh);
});
```

### 5.5 Admin Dashboard: Payment Management

| Component | Mô tả |
|-----------|-------|
| Danh sách giao dịch chờ | Bảng lọc `trangthai = 'ChoThanhToan'` |
| Nút [✅ Xác nhận] | Mock webhook, cập nhật real-time |
| Lịch sử giao dịch | Xem tất cả giao dịch đã xác nhận |
| Filter: Hôm nay / Tuần này / Tháng này | Date range picker |

### 5.6 Ma trận so sánh

| Tiêu chí | Hiện tại (text) | Giải pháp (QR + Webhook) |
|----------|----------------|--------------------------|
| Trải nghiệm user | Nhập tay → sai sót | Quét mã → tự động |
| Tốc độ xác nhận | Chờ admin kiểm tra thủ công | Admin click 1 nút |
| Real-time | Không | SignalR broadcast |
| Lưu lịch sử | Không | Bảng tbGiaoDichThanhToan |
| Sai sót | User nhập sai số tiền/nội dung | QR encode sẵn → chính xác |

---

## 6. Customer — Address Book

### 6.1 Vấn đề hiện tại

```
Current Address:
┌──────────────────────────────────────────────────┐
│  tbThongTinDatHang — Lưu địa chỉ theo đơn hàng  │
│  - Mỗi lần đặt hàng phải nhập lại địa chỉ        │
│  - Không có khái niệm "địa chỉ mặc định"          │
│  - Không phân loại (Nhà riêng / Cơ quan)         │
│  - Không thể quản lý danh sách địa chỉ            │
└──────────────────────────────────────────────────┘
```

### 6.2 Giải pháp: Bảng tbDiaChiKhachHang

#### Bảng mới

```sql
CREATE TABLE tbDiaChiKhachHang (
    madiachi      INT AUTO_INCREMENT PRIMARY KEY,
    userid        INT NOT NULL,           -- FK → tbKhachHang.userid
    tenkhachhang  VARCHAR(100) NOT NULL,   -- Tên người nhận
    sdt           VARCHAR(11) NOT NULL,    -- SĐT người nhận
    diachi        VARCHAR(500) NOT NULL,   -- Địa chỉ chi tiết
    toado         VARCHAR(50),             -- Tọa độ GPS "lat,lng"
    loaidiachi    VARCHAR(20) DEFAULT 'NhaRieng',  -- 'NhaRieng', 'CoQuan', 'Khac'
    mamacdinh     BIT DEFAULT 0,           -- 1 = mặc định
    trangthai     BIT DEFAULT 1,           -- 1 = active, 0 = deleted (soft delete)
    ngaytao       DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (userid) REFERENCES tbKhachHang(userid),
    INDEX idx_userid (userid),
    INDEX idx_macdinh (userid, mamacdinh)
);
```

#### Quan hệ

```
tbKhachHang (1) ──── (N) tbDiaChiKhachHang
     │                        │
     │                        │
     ▼                        ▼
tbThongTinDatHang        (địa chỉ đã lưu)
  (địa chỉ theo đơn)
```

### 6.3 Logic xử lý

#### Thêm địa chỉ mới

```
User nhập địa chỉ → POST /Home/AddAddress
    │
    ▼
1. Validate input
2. Nếu là địa chỉ đầu tiên → tự động set mamacdinh = 1
3. Nếu user chọn "Đặt làm mặc định" → 
   - Tắt mamacdinh của tất cả địa chỉ cũ
   - Set mamacdinh = 1 cho địa chỉ mới
4. Lưu vào tbDiaChiKhachHang
5. Trả về JSON { success: true, madiachi: newId }
```

#### Checkout: Chọn địa chỉ

```
Checkout.cshtml:
┌──────────────────────────────────────────────┐
│ 📍 Địa chỉ giao hàng                          │
│                                               │
│ ┌──────────────────────────────────────────┐ │
│ │ ○ Nhà riêng (mặc định)                   │ │
│ │   123 Nguyễn Huệ, Q1, TP.HCM             │ │
│ │   Liên hệ: Nguyễn Văn A - 0901234567     │ │
│ ├──────────────────────────────────────────┤ │
│ │ ○ Cơ quan                                │ │
│ │   48 Cao Thắng, Q3, TP.HCM               │ │
│ │   Liên hệ: Nguyễn Văn A - 0901234567     │ │
│ ├──────────────────────────────────────────┤ │
│ │ [+ Thêm địa chỉ mới]                     │ │
│ └──────────────────────────────────────────┘ │
└──────────────────────────────────────────────┘
```

#### Sổ địa chỉ (Profile)

```
Trang quản lý địa chỉ: /Home/AddressBook
┌──────────────────────────────────────────────┐
│ 📒 Sổ địa chỉ                         [+ Mới]│
│                                               │
│ ┌──────────────────────────────────────────┐ │
│ │ 🏠 Nhà riêng            [Mặc định] ✏️ 🗑  │ │
│ │ 123 Nguyễn Huệ, Q1, TP.HCM               │ │
│ │ Nguyễn Văn A - 0901234567                 │ │
│ ├──────────────────────────────────────────┤ │
│ │ 🏢 Cơ quan                    ✏️ 🗑        │ │
│ │ 48 Cao Thắng, Q3, TP.HCM                 │ │
│ │ Nguyễn Văn A - 0901234567                 │ │
│ └──────────────────────────────────────────┘ │
└──────────────────────────────────────────────┘
```

### 6.4 Migration từ dữ liệu cũ

Khi triển khai, có thể migrate địa chỉ từ `tbThongTinDatHang` sang `tbDiaChiKhachHang`:

```sql
INSERT INTO tbDiaChiKhachHang (userid, tenkhachhang, sdt, diachi, toado, loaidiachi)
SELECT DISTINCT userid, tenkhach, sdtkhach, diachi, toado, 'Khac'
FROM tbThongTinDatHang
WHERE userid IS NOT NULL;
```

---

## 7. Customer — Hủy đơn & Hoàn tiền

### 7.1 Vấn đề hiện tại

```
Current Cancel Flow:
┌──────────────────────────────────────────────────┐
│  User muốn hủy đơn → Không có nút hủy           │
│  → Phải gọi điện/quán admin hủy giúp            │
│  → Nếu đã thanh toán: mất tiền                   │
└──────────────────────────────────────────────────┘
```

### 7.2 Giải pháp: Quy trình hủy đơn 3 phút

#### Luồng nghiệp vụ

```
User bấm [Hủy đơn] trên ChiTietDonHang
    │
    ▼
Kiểm tra điều kiện hủy:
  ├── Đã quá 3 phút kể từ khi đặt? → ❌ Không thể hủy
  ├── Quán đã xác nhận? → ❌ Không thể hủy
  └── Thoả mãn cả 2? → ✅ Cho phép hủy
    │
    ▼
Popup xác nhận: "Bạn có chắc muốn hủy đơn #123?"
    │
    ├── [Không] → Đóng popup
    │
    └── [Có, hủy đơn] →
         │
         ▼
    1. Cập nhật tbDonHang.trangthai = "Đã hủy"
    2. Thêm ghi chú hủy: "Khách hủy trong 3 phút"
    3. Nếu đơn đã thanh toán (trạng thái "Đã thanh toán"):
         └── Tạo bản ghi hoàn tiền:
              tbGiaoDichThanhToan (madh, soTien, 
                  trangthai = 'HoanTien', ngaytao = now)
         └── Cộng lại số tiền vào ví ảo user:
              UPDATE tbUser SET vitien = vitien + @soTien
              WHERE userid = @userId
    4. SignalR broadcast đến quán + admin: "Đơn #123 đã hủy"
    5. Hiển thị toast: "✅ Hủy đơn thành công!"
```

#### Cập nhật trạng thái đơn hàng

| Trạng thái | Mô tả | Có thể hủy? |
|-----------|-------|-------------|
| `Chờ xác nhận` | Vừa đặt, quán chưa xác nhận | ✅ Trong 3 phút |
| `Đã xác nhận` | Quán đã nhận đơn | ❌ |
| `Đang chuẩn bị` | Quán đang nấu | ❌ |
| `Đang giao` | Shipper đang giao | ❌ |
| `Hoàn thành` | Đã giao xong | ❌ |
| `Đã hủy` | Đã hủy | — |

#### Bảng ghi chú hủy đơn (tuỳ chọn)

```sql
CREATE TABLE tbHuyDon (
    mahuy         INT AUTO_INCREMENT PRIMARY KEY,
    madh          INT NOT NULL,
    lydohuy       VARCHAR(500),
    nguoihuy      VARCHAR(20) NOT NULL,  -- 'KhachHang', 'QuanAn', 'Admin'
    hoatien       BIT DEFAULT 0,         -- 1 = đã hoàn tiền
    sotienhoan    DECIMAL(18,0),
    ngayhuy       DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (madh) REFERENCES tbDonHang(madh)
);
```

#### UI Button

```html
<!-- ChiTietDonHang.cshtml -->
@if (donHang.trangthai == "Chờ xác nhận" 
     && (DateTime.Now - donHang.ngaydathang).TotalMinutes < 3)
{
    <button class="btn btn-danger" onclick="huyDon(@donHang.madh)">
        🚫 Hủy đơn (còn @calcularRemainingMinutes phút)
    </button>
}

<script>
function huyDon(madh) {
    if (!confirm("Bạn có chắc muốn hủy đơn #" + madh + "?")) return;
    
    $.post('/Home/CancelOrder', { madh: madh })
        .done(function(res) {
            if (res.success) {
                showToast('✅ ' + res.message);
                location.reload();
            } else {
                showToast('❌ ' + res.message);
            }
        });
}
</script>
```

### 7.3 Xử lý hoàn tiền

#### Logic hoàn tiền

| Kịch bản | Hành động |
|----------|-----------|
| COD + hủy trong 3 phút | Chỉ hủy đơn, không liên quan tiền |
| Chuyển khoản + hủy trong 3 phút | Hủy đơn + hoàn tiền vào ví ảo `tbUser.vitien` |
| Chuyển khoản + quá 3 phút | Không thể hủy, liên hệ admin |
| COD + quán đã xác nhận | Không thể hủy |

#### Ví ảo (Virtual Wallet)

`tbUser.vitien` đã tồn tại — dùng làm ví ảo:

| Sự kiện | Ghi Nợ | Ghi Có |
|---------|--------|--------|
| Thanh toán online | — | Trừ số tiền |
| Hủy đơn + hoàn tiền | Cộng lại | — |
| Admin cộng tiền thủ công | — | Cộng |

---

## 8. Restaurant — Preparing Status

### 8.1 Vấn đề hiện tại

```
Current Order Flow:
┌──────────────────────────────────────────────────┐
│  Đặt hàng → Chờ xác nhận → Đang giao → Hoàn thành│
│                              ↑                    │
│                     Bỏ qua bước "Đang chuẩn bị"    │
│                     → Shipper nhận đơn ngay       │
│                     → Không có thời gian nấu      │
└──────────────────────────────────────────────────┘
```

### 8.2 Giải pháp: Thêm trạng thái "Đang chuẩn bị món"

```
Target Order Flow:
┌──────────────────────────────────────────────────┐
│  Đặt hàng                                        │
│    │                                             │
│    ▼                                             │
│  Chờ xác nhận ← Quán có thể từ chối               │
│    │                                             │
│    ▼                                             │
│  ✅ Đã xác nhận                                   │
│    │                                             │
│    ▼                                             │
│  👨‍🍳 Đang chuẩn bị món (Preparing)  ← MỚI        │
│    │                  ↑                          │
│    │    Quán bấm "Hoàn thành chuẩn bị"           │
│    │    → SignalR phát tín hiệu tìm Shipper      │
│    ▼                                             │
│  🚚 Đang giao ← Auto-Matching gán Shipper        │
│    │                                             │
│    ▼                                             │
│  ✅ Hoàn thành                                    │
└──────────────────────────────────────────────────┘
```

### 8.3 ✅ Tình trạng thi công

**ĐÃ IMPLEMENT** (Tháng 7/2026):
- `Services/AutoPreparingService.cs`: BackgroundService poll 10s, đơn "Đã xác nhận" quá 5s → tự động chuyển "Đang chuẩn bị" → SignalR `newOrderReady`
- `Program.cs`: `builder.Services.AddHostedService<AutoPreparingService>()`
- **Trạng thái**: ✅ Đã code — tự động chạy khi app start

### 8.4 Chi tiết triển khai

#### Cập nhật enum trạng thái

```csharp
// Trong Model tbDonHang.cs, thêm constants
public static class TrangThaiDonHang
{
    public const string ChoXacNhan  = "Chờ xác nhận";
    public const string DaXacNhan   = "Đã xác nhận";
    public const string DangChuanBi = "Đang chuẩn bị";  // MỚI
    public const string DangGiao    = "Đang giao";
    public const string HoanThanh   = "Hoàn thành";
    public const string DaHuy       = "Đã hủy";
}
```

#### SignalR: Tìm Shipper tự động

```csharp
// RestaurantController.cs — Quán bấm "Hoàn thành chuẩn bị"
[HttpPost]
public async Task<IActionResult> CompletePreparing(int madh)
{
    var donHang = await _db.tbDonHang.FindAsync(madh);
    donHang.trangthai = TrangThaiDonHang.DangChuanBi;
    await _db.SaveChangesAsync();
    
    // Gửi tín hiệu đến tất cả Shipper đang online
    // → Auto-Matching Service sẽ xử lý (xem Task 11)
    await _hubContext.Clients.All.SendAsync("newOrderReady", madh, donHang.maquan);
    
    return Json(new { success = true });
}
```

#### UI: Nút "Hoàn thành chuẩn bị"

```html
<!-- Restaurant/OrderList.cshtml -->
@if (donHang.trangthai == "Đã xác nhận")
{
    <button class="btn btn-primary btn-sm" onclick="completePreparing(@donHang.madh)">
        👨‍🍳 Hoàn thành chuẩn bị
    </button>
}
```

---

## 9. Restaurant — Hết hàng nhanh

### 9.1 Vấn đề hiện tại

```
Current Toggle:
┌──────────────────────────────────────────────────┐
│  Muốn tắt món "Phở bò":                          │
│  1. Vào ProductList                              │
│  2. Click [Sửa]                                   │
│  3. Scroll tìm checkbox "Còn hàng"               │
│  4. Bỏ tick                                      │
│  5. Click [Lưu]                                   │
│  → Mất ~5 click, chậm khi quán đông khách        │
└──────────────────────────────────────────────────┘
```

### 9.2 Giải pháp: AJAX Toggle 1-click

```
Target Toggle:
┌──────────────────────────────────────────────────┐
│  Danh sách món ăn (Restaurant/ProductList)       │
│                                                   │
│  🍜 Phở bò          35.000đ  🔴 Còn hàng         │
│  🍚 Cơm tấm         30.000đ  🔴 Còn hàng         │
│  🥤 Trà sữa         20.000đ  ⚪ Hết hàng         │
│                                                   │
│  Click vào 🔴 → AJAX → ⚪ Hết hàng               │
│  Click vào ⚪ → AJAX → 🔴 Còn hàng               │
│  → Không reload page                              │
└──────────────────────────────────────────────────┘
```

### 9.3 ✅ Tình trạng thi công

**ĐÃ IMPLEMENT** (Tháng 7/2026):
- `Models/tbMonAn.cs`: Thêm property `conhang` (bool, default true)
- `Models/DataAnalytic.cs`: Thêm property `conhang` cho view display
- `Controllers/RestaurantController.cs`: Thêm `ToggleConHang(int mamon)` — POST API
- `Views/Restaurant/ProductList.cshtml`: Nút toggle AJAX, toast notification
- `Program.cs`: Auto-migration SQL thêm cột `conhang BIT DEFAULT 1`
- **Trạng thái**: ✅ Đã code — chủ quán click 1 lần để bật/tắt trạng thái còn hàng

---

## 10. Restaurant — Giờ hoạt động tự động

### 10.1 Vấn đề hiện tại

```
Current Schedule:
┌──────────────────────────────────────────────────┐
│  Quán tự bấm nút "Mở cửa" / "Đóng cửa" thủ công │
│  → Quên bấm → Khách đặt hàng lúc đóng cửa       │
│  → Bấm không kịp → Mất đơn khi đến giờ mở       │
└──────────────────────────────────────────────────┘
```

### 10.2 Giải pháp: Bảng cấu hình giờ tự động

#### Bảng mới

```sql
CREATE TABLE tbGioHoatDong (
    magio       INT AUTO_INCREMENT PRIMARY KEY,
    maquanan    INT NOT NULL,
    thuhai      VARCHAR(20) DEFAULT '07:00-21:30',  -- "HH:mm-HH:mm" hoặc "DongCua"
    thuba       VARCHAR(20) DEFAULT '07:00-21:30',
    thutu       VARCHAR(20) DEFAULT '07:00-21:30',
    thunam      VARCHAR(20) DEFAULT '07:00-21:30',
    thusau      VARCHAR(20) DEFAULT '07:00-21:30',
    thubay      VARCHAR(20) DEFAULT '08:00-22:00',
    chunhat     VARCHAR(20) DEFAULT '08:00-22:00',
    
    FOREIGN KEY (maquanan) REFERENCES tbQuanAn(maquanan)
);
```

#### Logic tự động

```
Scheduler (BackgroundService):
┌──────────────────────────────────────────────────┐
│  Mỗi phút chạy 1 lần:                           │
│  1. Lấy tất cả quán có tbGioHoatDong            │
│  2. Với mỗi quán:                               │
│     - Xác định thứ hiện tại                      │
│     - Parse giờ mở / giờ đóng từ config          │
│     - So sánh với giờ hiện tại                   │
│     - Nếu trong giờ → tbQuanAn.trangthaiquan = 1 │
│     - Nếu ngoài giờ → tbQuanAn.trangthaiquan = 0 │
│  3. Update DB                                    │
└──────────────────────────────────────────────────┘
```

#### UI: Cấu hình giờ

```html
<!-- Restaurant/Profile.cshtml — tab "Giờ hoạt động" -->
┌──────────────────────────────────────────────┐
│ ⏰ Giờ hoạt động                              │
│                                               │
│ Thứ Hai        [07:00] ─ [21:30]  ☑ Mở cửa  │
│ Thứ Ba         [07:00] ─ [21:30]  ☑ Mở cửa  │
│ Thứ Tư         [07:00] ─ [21:30]  ☑ Mở cửa  │
│ Thứ Năm        [07:00] ─ [21:30]  ☑ Mở cửa  │
│ Thứ Sáu        [07:00] ─ [21:30]  ☑ Mở cửa  │
│ Thứ Bảy        [08:00] ─ [22:00]  ☑ Mở cửa  │
│ Chủ Nhật       [08:00] ─ [22:00]  ☑ Mở cửa  │
│                                               │
│          [💾 Lưu cấu hình]                    │
└──────────────────────────────────────────────┘
```

#### Background Service

```csharp
// Services/AutoScheduleService.cs
public class AutoScheduleService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var today = now.DayOfWeek;
            var currentTime = now.ToString("HH:mm");

            // Lấy tất cả quán có cấu hình giờ
            var schedules = await _db.tbGioHoatDongs.ToListAsync();

            foreach (var schedule in schedules)
            {
                var timeRange = GetTimeRange(schedule, today);
                if (string.IsNullOrEmpty(timeRange) || timeRange == "DongCua")
                {
                    // Đóng cửa
                    await SetTrangThai(schedule.maquanan, 0);
                    continue;
                }

                var parts = timeRange.Split('-');
                if (parts.Length == 2)
                {
                    var isOpen = string.Compare(currentTime, parts[0]) >= 0
                              && string.Compare(currentTime, parts[1]) <= 0;
                    await SetTrangThai(schedule.maquanan, isOpen ? 1 : 0);
                }
            }

            // Chạy mỗi 5 phút (không cần mỗi giây)
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private string? GetTimeRange(tbGioHoatDong schedule, DayOfWeek day)
        => day switch
        {
            DayOfWeek.Monday    => schedule.thuhai,
            DayOfWeek.Tuesday   => schedule.thuba,
            DayOfWeek.Wednesday => schedule.thutu,
            DayOfWeek.Thursday  => schedule.thunam,
            DayOfWeek.Friday    => schedule.thusau,
            DayOfWeek.Saturday  => schedule.thubay,
            DayOfWeek.Sunday    => schedule.chunhat,
            _ => null
        };

    private async Task SetTrangThai(int maquanan, int trangthai)
    {
        var quan = await _db.tbQuanAns.FindAsync(maquanan);
        if (quan != null && quan.trangthaiquan != trangthai)
        {
            quan.trangthaiquan = trangthai;
            await _db.SaveChangesAsync();
        }
    }
}
```

---

## 11. Shipper — Auto-Matching

### 11.1 Vấn đề hiện tại

```
Current FREE-PICK:
┌──────────────────────────────────────────────────┐
│  Đơn hàng mới → Tất cả Shipper đều thấy          │
│  → Shipper nhanh tay nhất giành được đơn         │
│  → Shipper ở xa vẫn giành được                   │
│  → Khách chờ lâu hơn                             │
│  → Không công bằng                               │
└──────────────────────────────────────────────────┘
```

### 11.2 Giải pháp: GPS-based Auto-Matching

```
Target Auto-Matching:
┌──────────────────────────────────────────────────┐
│  Quán bấm "Hoàn thành chuẩn bị"                  │
│    │                                              │
│    ▼                                              │
│  System gọi Auto-Matching Service:                │
│    │                                              │
│    1. Lấy toạ độ quán (tbQuanAn.toado)           │
│    2. Lấy danh sách Shipper đang rảnh (online)   │
│    3. Tính khoảng cách từ mỗi Shipper đến quán   │
│    4. Lọc Shipper trong bán kính < 3km           │
│    5. Sort theo khoảng cách tăng dần             │
│    6. Chọn Shipper gần nhất                      │
│    │                                              │
│    ▼                                              │
│  Gán đơn cho Shipper đó:                          │
│    - tbDonHang.mashipper = shipperId             │
│    - tbDonHang.trangthai = "Đang giao"           │
│    - SignalR: Notify shipper "Bạn có đơn mới"     │
│    - SignalR: Notify customer "Đơn đang được giao"│
└──────────────────────────────────────────────────┘
```

### 11.3 Thuật toán Haversine

```csharp
// Utils/GeoUtils.cs
public static class GeoUtils
{
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Tính khoảng cách giữa 2 điểm GPS (Haversine formula)
    /// </summary>
    public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        return EarthRadiusKm * c;
    }

    private static double ToRadians(double angle) => Math.PI * angle / 180.0;
}
```

### 11.4 Auto-Matching Service

```csharp
// Services/AutoMatchingService.cs
public class AutoMatchingService
{
    private readonly dbFoodyEntities _db;
    private readonly IHubContext<Chats> _hubContext;

    public AutoMatchingService(dbFoodyEntities db, IHubContext<Chats> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task<tbShipper?> FindAndAssignShipper(int madh)
    {
        var donHang = await _db.tbDonHangs
            .Include(d => d.tbQuanAn)
            .FirstOrDefaultAsync(d => d.madh == madh);

        if (donHang?.tbQuanAn?.toado == null) return null;

        // Parse toạ độ quán
        var quanToado = donHang.tbQuanAn.toado.Split(',');
        if (quanToado.Length != 2) return null;

        double quanLat = double.Parse(quanToado[0]);
        double quanLng = double.Parse(quanToado[1]);

        // Lấy tất cả Shipper đang online + rảnh
        var shippers = await _db.tbUsers
            .Where(u => u.loaitaikhoan == "Shipper" && u.trangthai == 1 && Chats.IsUserOnline(u.userid))
            .Join(_db.tbShippers, u => u.userid, s => s.userid, (u, s) => s)
            .ToListAsync();

        // Tính khoảng cách + lọc < 3km + sort
        var nearest = shippers
            .Select(s =>
            {
                var toado = s.toado?.Split(',');
                if (toado == null || toado.Length != 2) return null;

                double sLat = double.Parse(toado[0]);
                double sLng = double.Parse(toado[1]);
                double dist = GeoUtils.GetDistance(quanLat, quanLng, sLat, sLng);

                return new { Shipper = s, Distance = dist };
            })
            .Where(x => x != null && x.Distance <= 3.0) // < 3km
            .OrderBy(x => x!.Distance)
            .FirstOrDefault();

        if (nearest == null) return null;

        // Gán đơn
        donHang.mashipper = nearest.Shipper.mashipper;
        donHang.trangthai = "Đang giao";
        await _db.SaveChangesAsync();

        // SignalR: Notify Shipper
        var connId = Chats.GetUserConnectionId(nearest.Shipper.userid);
        if (connId != null)
        {
            await _hubContext.Clients.Client(connId)
                .SendAsync("orderAssigned", madh, donHang.tbQuanAn.tenquan);
        }

        return nearest.Shipper;
    }
}
```

### 11.5 Service Registration

```csharp
// Program.cs
builder.Services.AddScoped<AutoMatchingService>();
```

---

## 12. Shipper — Rút tiền

### 12.1 Vấn đề hiện tại

```
Current Wallet:
┌──────────────────────────────────────────────────┐
│  tbShipper: có trường doanhthu, dakinhan          │
│  → Chỉ hiển thị số dư                            │
│  → Không có chức năng rút tiền                    │
│  → Shipper muốn nhận tiền phải liên hệ admin      │
└──────────────────────────────────────────────────┘
```

### 12.2 Giải pháp: Bảng yêu cầu rút tiền

#### Bảng mới

```sql
CREATE TABLE tbYeuCauRutTien (
    maruttien    INT AUTO_INCREMENT PRIMARY KEY,
    mashipper    INT NOT NULL,
    sotien       DECIMAL(18,0) NOT NULL,
    sotk         VARCHAR(50) NOT NULL,    -- Số tài khoản ngân hàng
    tentk        VARCHAR(100) NOT NULL,   -- Tên chủ tài khoản
    tennganhang  VARCHAR(100) NOT NULL,   -- Tên ngân hàng
    trangthai    VARCHAR(20) DEFAULT 'ChoDuyet',
      -- 'ChoDuyet', 'DaDuyet', 'DaGiaiNgan', 'TuChoi'
    lydo         VARCHAR(500),            -- Lý do từ chối (nếu có)
    ngayyeucau   DATETIME DEFAULT CURRENT_TIMESTAMP,
    ngayxuly     DATETIME,
    nguoixuly    INT,                     -- admin id

    FOREIGN KEY (mashipper) REFERENCES tbShipper(mashipper),
    FOREIGN KEY (nguoixuly) REFERENCES tbAdmin(maadmin),
    INDEX idx_mashipper (mashipper),
    INDEX idx_trangthai (trangthai)
);
```

### 12.3 Luồng nghiệp vụ

```
Shipper vào Ví Tiền → [Rút tiền]
    │
    ▼
Form rút tiền:
  ┌──────────────────────────────┐
  │ 💰 Số dư hiện tại: 500.000đ  │
  │                              │
  │ Số tiền muốn rút: [200.000]  │
  │ Ngân hàng:    [Vietcombank ▼]│
  │ Số tài khoản: [1903xxxxxx]   │
  │ Chủ tài khoản: [Nguyễn Văn A]│
  │                              │
  │  [Gửi yêu cầu rút tiền]     │
  └──────────────────────────────┘
    │
    ▼
System: Kiểm tra số dư đủ?
  ├── Không đủ → Báo lỗi
  └── Đủ → Tạo yêu cầu (trangthai = 'ChoDuyet')
          → Trừ tạm vào số dư (hoặc chờ duyệt mới trừ)
    │
    ▼
Admin Dashboard → Quản lý rút tiền
  ┌──────────────────────────────────────────┐
  │ Danh sách yêu cầu rút tiền chờ duyệt     │
  │                                           │
  │ Shipper A - 200.000đ - 12/07/2026        │
  │   [✅ Duyệt] [❌ Từ chối]                 │
  │                                           │
  │ Shipper B - 500.000đ - 11/07/2026        │
  │   [✅ Duyệt] [❌ Từ chối]                 │
  └──────────────────────────────────────────┘
    │
    ├── Duyệt → Cập nhật trangthai = 'DaDuyet'
    │           → Trừ số dư shipper
    │           → Ghi chú: "Chờ giải ngân"
    │
    └── Từ chối → Cập nhật trangthai = 'TuChoi'
                  → Ghi lý do
                  → Cộng lại số dư cho shipper
```

### 12.4 UI: Lịch sử rút tiền

```html
<!-- Shipper/ViTien.cshtml — tab "Lịch sử rút tiền" -->
┌──────────────────────────────────────────────┐
│ 📋 Lịch sử rút tiền                          │
│                                               │
│ 12/07  Vietcombank       200.000đ  ⏳ Chờ    │
│ 05/07  Vietcombank       500.000đ  ✅ Đã duyệt│
│ 28/06  Techcombank       300.000đ  ✅ Đã duyệt│
│ 15/06  Vietcombank       200.000đ  ❌ Từ chối │
│                           (Số TK không đúng)  │
└──────────────────────────────────────────────┘
```

---

## 13. Shipper — Proof of Delivery (POD)

### 13.1 Vấn đề hiện tại

```
Current Delivery:
┌──────────────────────────────────────────────────┐
│  Shipper giao hàng → Bấm "Hoàn thành"           │
│  → Không có bằng chứng                          │
│  → Khách hàng nói "Chưa nhận được"              │
│  → Không có ảnh để đối soát                     │
│  → Shipper dễ bị tố cáo gian lận                │
└──────────────────────────────────────────────────┘
```

### 13.2 Giải pháp: Upload ảnh POD

#### Thêm cột vào tbDonHang

```sql
ALTER TABLE tbDonHang 
  ADD COLUMN hinhgiaohang VARCHAR(500) AFTER trangthai,
  ADD COLUMN ghichugiao VARCHAR(500) AFTER hinhgiaohang,
  ADD COLUMN nguoinhan VARCHAR(100) AFTER ghichugiao;
```

#### Luồng nghiệp vụ

```
Shipper bấm [Hoàn thành] trên OrderDetail
    │
    ▼
Popup POD:
┌──────────────────────────────────────────────┐
│ 📸 Xác nhận giao hàng                        │
│                                               │
│  ┌────────────────────────────────────┐       │
│  │                                    │       │
│  │      [📷 Chụp ảnh xác nhận]       │       │
│  │       hoặc [📁 Chọn từ thư viện]   │       │
│  │                                    │       │
│  └────────────────────────────────────┘       │
│                                               │
│  Tên người nhận: [_______________]            │
│  Ghi chú: [Đã giao tận tay]                   │
│                                               │
│  [✅ Xác nhận hoàn thành]   [Hủy]            │
└──────────────────────────────────────────────┘
    │
    ▼
Xử lý:
  1. Upload ảnh lên server (wwwroot/uploads/pod/)
  2. Lưu đường dẫn vào tbDonHang.hinhgiaohang
  3. Lưu tên người nhận + ghi chú
  4. Cập nhật trạng thái → "Hoàn thành"
  5. SignalB: Gửi thông báo đến khách hàng + admin
```

#### API Upload

```csharp
// ShipperController.cs
[HttpPost]
public async Task<IActionResult> CompleteDelivery(int madh, 
    IFormFile? hinhAnh, string nguoiNhan, string ghiChu)
{
    var donHang = await _db.tbDonHangs.FindAsync(madh);
    if (donHang == null) return Json(new { success = false });

    // Xử lý upload ảnh
    string? imagePath = null;
    if (hinhAnh != null && hinhAnh.Length > 0)
    {
        var uploadsDir = Path.Combine(appEnvironment.WebRootPath, "uploads", "pod");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"pod_{madh}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await hinhAnh.CopyToAsync(stream);

        imagePath = $"/uploads/pod/{fileName}";
    }

    // Cập nhật đơn hàng
    donHang.trangthai = "Hoàn thành";
    donHang.hinhgiaohang = imagePath;
    donHang.nguoinhan = nguoiNhan;
    donHang.ghichugiao = ghiChu;
    donHang.ngaygiao = DateTime.Now;
    await _db.SaveChangesAsync();

    // SignalR: thông báo khách hàng
    await _hubContext.Clients
        .Group($"order_{madh}")
        .SendAsync("deliveryCompleted", madh, nguoiNhan);

    return Json(new { success = true });
}
```

---

## 14. Admin — Duyệt đối tác

### 14.1 Vấn đề hiện tại

```
Current Registration:
┌──────────────────────────────────────────────────┐
│  Đăng ký tài khoản mới → Tự động active         │
│  → Quán ăn không có thực phẩm vẫn được đăng ký  │
│  → Shipper không có giấy tờ vẫn active          │
│  → Không có quy trình kiểm duyệt                 │
└──────────────────────────────────────────────────┘
```

### 14.2 Giải pháp: Trạng thái "Chờ duyệt" + Dashboard duyệt

#### Thêm cột trạng thái

```sql
-- Mở rộng tbUser.trangthai:
-- 0 = Chờ duyệt (MỚI)
-- 1 = Hoạt động
-- -1 = Khóa

ALTER TABLE tbUser MODIFY COLUMN trangthai INT DEFAULT 0;
```

#### Bảng hồ sơ đăng ký

```sql
CREATE TABLE tbHoSoDangKy (
    mahoso       INT AUTO_INCREMENT PRIMARY KEY,
    userid       INT NOT NULL,
    loaihoso     VARCHAR(20) NOT NULL,  -- 'QuanAn', 'Shipper'
    ten          VARCHAR(200) NOT NULL,
    diachi       VARCHAR(500),
    sdt          VARCHAR(11),
    email        VARCHAR(100),
    giayphep     VARCHAR(500),           -- Đường dẫn file giấy phép / CMND
    ghichu       VARCHAR(500),
    trangthai    VARCHAR(20) DEFAULT 'ChoDuyet',
      -- 'ChoDuyet', 'DaDuyet', 'TuChoi'
    nguoiduyet   INT,
    ngaydangky   DATETIME DEFAULT CURRENT_TIMESTAMP,
    ngayduyet    DATETIME,

    FOREIGN KEY (userid) REFERENCES tbUser(userid),
    FOREIGN KEY (nguoiduyet) REFERENCES tbAdmin(maadmin)
);
```

### 14.3 Luồng đăng ký mới

```
User đăng ký (Signup)
    │
    ▼
Nếu chọn "Đăng ký làm Đối tác":
  ┌──────────────────────────────────────────────┐
  │ 📝 Đăng ký đối tác FastShip                  │
  │                                               │
  │ Bạn muốn đăng ký với vai trò nào?             │
  │  ○ Quán ăn → Điền thông tin quán             │
  │  ○ Shipper → Điền thông tin cá nhân          │
  │                                               │
  │ [Tiếp theo]                                   │
  └──────────────────────────────────────────────┘
    │
    ▼
Register flow:
  1. Tạo tbUser (trangthai = 0 = Chờ duyệt)
  2. Tạo tbHoSoDangKy (trangthai = 'ChoDuyet')
  3. Tạo tbQuanAn hoặc tbShipper (chưa active)
  4. Hiển thị: "Tài khoản của bạn đang chờ duyệt"
```

#### Admin Dashboard: Duyệt đối tác

```
Admin → Quản lý Đối tác
┌──────────────────────────────────────────────┐
│ 🔍 Đối tác chờ duyệt              ⏳ 5 hồ sơ │
│                                               │
│ ┌──────────────────────────────────────────┐ │
│ │ 🏪 Quán ăn                               │ │
│ │ Phở 24 - 48 Cao Thắng, Q3               │ │
│ │ Liên hệ: Nguyễn Văn A - 0901234567       │ │
│ │ [Xem giấy phép] [✅ Duyệt] [❌ Từ chối]  │ │
│ │                                           │ │
│ │ 🚚 Shipper                               │ │
│ │ Trần Văn B - 0912345678                   │ │
│ │ Biển số: 59A1-12345                       │ │
│ │ [Xem CMND] [✅ Duyệt] [❌ Từ chối]       │ │
│ └──────────────────────────────────────────┘ │
└──────────────────────────────────────────────┘
```

#### Xử lý duyệt/từ chối

```csharp
// AdminController.cs
[HttpPost]
public async Task<IActionResult> DuyetDoiTac(int mahoso, bool duyet, string? lydo)
{
    var hoSo = await _db.tbHoSoDangKys
        .Include(h => h.tbUser)
        .FirstAsync(h => h.mahoso == mahoso);

    if (duyet)
    {
        // Duyệt: active user
        hoSo.tbUser.trangthai = 1;
        hoSo.trangthai = "DaDuyet";
        hoSo.nguoiduyet = adminId;
        hoSo.ngayduyet = DateTime.Now;
    }
    else
    {
        // Từ chối
        hoSo.trangthai = "TuChoi";
        hoSo.lydo = lydo;
    }

    await _db.SaveChangesAsync();
    return Json(new { success = true });
}
```

---

## 15. Admin — Đối soát & Chiết khấu

### 15.1 Vấn đề hiện tại

```
Current Financial:
┌──────────────────────────────────────────────────┐
│  Hệ thống chưa có:                              │
│  - Không tính chiết khấu tự động                │
│  - Không có bảng đối soát tuần                  │
│  - Không biết quán nợ bao nhiêu                 │
│  - Shipper không biết thu nhập thực tế          │
└──────────────────────────────────────────────────┘
```

### 15.2 Mô hình chiết khấu

```
Revenue Sharing Model:
┌──────────────────────────────────────────────────┐
│  Đơn hàng giá 100.000đ + phí ship 15.000đ       │
│                                                   │
│  DOANH THU (115.000đ)                             │
│    │                                               │
│    ├── Quán ăn (85% doanh thu món)                │
│    │   100.000đ × 85% = 85.000đ                  │
│    │                                              │
│    ├── Shipper (85% phí ship)                    │
│    │   15.000đ × 85% = 12.750đ                   │
│    │                                              │
│    └── Hệ thống FastShip (15% món + 15% ship)   │
│        100.000đ × 15% + 15.000đ × 15% = 17.250đ │
└──────────────────────────────────────────────────┘
```

### 15.3 Bảng đối soát

```sql
CREATE TABLE tbDoiSoat (
    madoisoat    INT AUTO_INCREMENT PRIMARY KEY,
    madh         INT NOT NULL,
    maquan       INT NOT NULL,
    mashipper    INT,
    tongtien     DECIMAL(18,0) NOT NULL,    -- Tổng giá trị đơn
    phiship      DECIMAL(18,0) DEFAULT 0,
    doanhthuquan DECIMAL(18,0) NOT NULL,    -- 85% tiền món
    doanhthuship DECIMAL(18,0),             -- 85% phí ship
    doanhthuht   DECIMAL(18,0) NOT NULL,    -- 15% món + 15% ship
    tuannay      INT NOT NULL,              -- Số tuần trong năm (1-52)
    nam          INT NOT NULL DEFAULT YEAR(CURDATE()),
    ngaythanhtoan DATETIME,
    trangthai    VARCHAR(20) DEFAULT 'ChuaThanhToan',
      -- 'ChuaThanhToan', 'DaThanhToan', 'DaHuy'

    FOREIGN KEY (madh) REFERENCES tbDonHang(madh),
    FOREIGN KEY (maquan) REFERENCES tbQuanAn(maquanan),
    FOREIGN KEY (mashipper) REFERENCES tbShipper(mashipper),
    INDEX idx_tuan (tuannay, nam)
);
```

### 15.4 Service tính toán tự động

```csharp
// Services/SettlementService.cs
public class SettlementService
{
    private const decimal TyLeChiKhauMonAn = 0.15m;    // 15% từ quán
    private const decimal TyLeChiKhauPhiShip = 0.15m;  // 15% từ ship

    public async Task TaoDoiSoatChoDonHang(tbDonHang donHang)
    {
        var tongTien = donHang.tongtien ?? 0;
        var phiShip = 15000m; // Phí ship cố định
        var tienMon = tongTien - phiShip;

        var doiSoat = new tbDoiSoat
        {
            madh = donHang.madh,
            maquan = donHang.maquan ?? 0,
            mashipper = donHang.mashipper,
            tongtien = tongTien,
            phiship = phiShip,
            doanhthuquan = tienMon * (1 - TyLeChiKhauMonAn),    // 85%
            doanhthuship = phiShip * (1 - TyLeChiKhauPhiShip),  // 85%
            doanhthuht = tienMon * TyLeChiKhauMonAn + phiShip * TyLeChiKhauPhiShip, // 15%
            tuannay = GetWeekNumber(DateTime.Now),
            nam = DateTime.Now.Year,
            trangthai = "ChuaThanhToan"
        };

        _db.tbDoiSoats.Add(doiSoat);
        await _db.SaveChangesAsync();
    }

    private static int GetWeekNumber(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}
```

### 15.5 Admin Dashboard: Đối soát

```
Admin → Tài chính → Đối soát
┌──────────────────────────────────────────────┐
│ 📊 Đối soát tuần 28/2026 (08-14/07)          │
│                                               │
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐         │
│ │ 215  │ │ 18.3M│ │ 15.5M│ │ 2.8M │         │
│ │ Đơn  │ │ DS   │ │ Quán │ │ HT   │         │
│ │      │ │      │ │ 85%  │ │ 15%  │         │
│ └──────┘ └──────┘ └──────┘ └──────┘         │
│                                               │
│ Chi tiết đối soát:                            │
│ ┌──────────────────────────────────────────┐ │
│ │ Đơn | Quán | Ship | Quán(85) | Ship(85) │ │
│ │ #201 | 100k | 15k | 85k   | 12.7k       │ │
│ │ #202 | 200k | 15k | 170k  | 12.7k       │ │
│ │ ...                                      │ │
│ │ ════════════════════════════════════════ │ │
│ │ Tổng: 18.3M         15.5M     | 2.8M    │ │
│ │                         (Chưa thanh toán) │ │
│ └──────────────────────────────────────────┘ │
│                                               │
│ [📥 Export CSV] [📨 Gửi báo cáo]              │
└──────────────────────────────────────────────┘
```

### 15.6 Quy trình thanh toán hàng tuần

```
Cuối tuần (Chủ nhật 23:59):
┌──────────────────────────────────────────────┐
│  System tự động tổng kết:                     │
│  1. Lấy tất cả đơn hoàn thành trong tuần      │
│  2. Tính doanh thu từng quán, từng shipper    │
│  3. Ghi vào bảng tbDoiSoat (nếu chưa có)      │
│                                               │
│  Admin kiểm tra → Export CSV                  │
│  → Gửi báo cáo cho kế toán                    │
│  → Bấm "Xác nhận thanh toán"                  │
│  → Cập nhật trangthai = 'DaThanhToan'        │
└──────────────────────────────────────────────┘
```

---

## Tổng Kết

### Impact Matrix

| # | Giải pháp | Complexity | Effort (man-days) | Priority | Status |
|---|-----------|-----------|-------------------|----------|--------|
| 1 | Redis Session | Thấp | 0.5 | 🔴 Cao | ✅ Đã code |
| 2 | EF Core Migrations | Trung bình | 1 | 🔴 Cao | ✅ Đã code |
| 3 | Rate Limiting | Thấp | 0.5 | 🔴 Cao | ✅ Đã code |
| 4 | Centralized Logging | Thấp | 0.5 | 🟡 Trung bình | ⬜ Chưa làm |
| 5 | QR Payment & Webhook | Trung bình | 2 | 🟡 Trung bình | 🟡 Code 1/2 (Mock Webhook có SignalR) |
| 6 | Address Book | Trung bình | 1 | 🟢 Thấp (UX) | ⬜ Chưa làm |
| 7 | Hủy đơn & Hoàn tiền | Trung bình | 1.5 | 🟡 Trung bình | ⬜ Chưa làm |
| 8 | Preparing Status | Thấp | 0.5 | 🟡 Trung bình | ✅ Đã code (AutoPreparingService) |
| 9 | Hết hàng nhanh | Thấp | 0.25 | 🟢 Thấp (UX) | ✅ Đã code (ToggleConHang) |
| 10 | Giờ tự động | Trung bình | 1 | 🟢 Thấp | ⬜ Chưa làm |
| 11 | Auto-Matching | Cao | 3 | 🟡 Trung bình | ⬜ Chưa làm |
| 12 | Rút tiền Shipper | Trung bình | 1.5 | 🟡 Trung bình | ⬜ Chưa làm |
| 13 | Proof of Delivery | Trung bình | 1 | 🟢 Thấp | ⬜ Chưa làm |
| 14 | Duyệt đối tác | Cao | 2 | 🟡 Trung bình | ⬜ Chưa làm |
| 15 | Đối soát & Chiết khấu | Cao | 3 | 🔴 Cao | ⬜ Chưa làm |

**Tổng effort ước tính**: ~18.75 man-days  
**Đã thi công**: 5/15 giải pháp  
**Khuyến nghị ưu tiên**: Code Debt (1-4) → Core Business (7-8-11) → Admin (14-15) → UX (5-6-9-10-12-13)

---

> **Document Version**: 1.1  
> **Cập nhật**: Tháng 7, 2026  
> **Tác giả**: Architectural Solution Document — Fastship (ShipFood)  
> **Tổng số giải pháp**: 15  
> **Đã thi công**: 5 (Redis, Migrations, Rate Limiting, Preparing Status, Hết hàng nhanh)  
> **Còn lại**: 10 giải pháp cần implement  
> **Tổng bảng DB mới**: 6 (tbGiaoDichThanhToan, tbDiaChiKhachHang, tbHuyDon, tbGioHoatDong, tbYeuCauRutTien, tbHoSoDangKy, tbDoiSoat)  
> **Tổng bảng mở rộng**: 3 (tbMonAn, tbDonHang, tbUser)
