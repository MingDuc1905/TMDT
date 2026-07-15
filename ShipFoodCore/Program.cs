using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using ShipFood.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

// ponytail: Cho phép DateTime Local với PostgreSQL timestamptz
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ─── Task 3: Centralized Logging (Serilog + Seq) ───
var logConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FastShip")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

Log.Logger = logConfig.CreateLogger();

// Chỉ ghi vào Seq nếu biến môi trường SEQ_URL được cấu hình
var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
if (!string.IsNullOrEmpty(seqUrl))
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "FastShip")
        .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.Seq(
            serverUrl: seqUrl,
            apiKey: Environment.GetEnvironmentVariable("SEQ_API_KEY") ?? null,
            period: TimeSpan.FromSeconds(5))
        .CreateLogger();
    Log.Information("Serilog Seq sink configured: {SeqUrl}", seqUrl);
}
else
{
    Log.Information("SEQ_URL not set — Seq sink skipped, logs go to Console only");
}

try
{
    Log.Information("Starting FastShip application...");

var builder = WebApplication.CreateBuilder(args);

// ─── FIX: Disable config file watching (inotify) to avoid Render free tier limit (128) ───
builder.Configuration.Sources.Clear();
var envName = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();
if (args.Length > 0)
    builder.Configuration.AddCommandLine(args);

// Use Serilog as the logging provider
builder.Host.UseSerilog();

// In production (Render), use PORT env var dynamically (Render also uses PORT)
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// ═══ Response compression (Gzip/Brotli) ═══
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);

// Add SignalR
builder.Services.AddSignalR();

// ─── Task 1a: Redis Distributed Session (fallback to In-Memory) ───
var redisConnection = builder.Configuration["Redis:Configuration"]
    ?? Environment.GetEnvironmentVariable("REDIS_URL");

if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "FastShip_Session:";
    });
    Console.WriteLine("[INFO] Redis distributed cache configured");
}
else
{
    // Fallback: in-memory (development, no Redis available)
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("[INFO] Using in-memory cache (no Redis configured)");
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ─── Task 1c: API Rate Limiting ───
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Gemini chatbot: 5 requests/minute per user
    options.AddFixedWindowLimiter("gemini-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Login: 60 requests in 5 minutes (sliding window)
    // ponytail: tăng từ 20→60 vì E2E tests chạy 10 files × ~3 login mỗi file = 30+ requests
    // Vẫn đủ an toàn vì bruteforce sẽ bị chặn ở tầng Render (firewall)
    options.AddSlidingWindowLimiter("login-policy", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.SegmentsPerWindow = 5;
        opt.QueueLimit = 0;
    });

    // General API: 100 requests/minute
    options.AddFixedWindowLimiter("general-api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Custom 429 JSON response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfter = context.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryAfterDuration)
            ? ((int)retryAfterDuration.TotalSeconds).ToString()
            : "60";

        context.HttpContext.Response.Headers["Retry-After"] = retryAfter;

        var response = System.Text.Json.JsonSerializer.Serialize(new
        {
            success = false,
            message = $"⚠️ Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau {retryAfter} giây.",
            retryAfterSeconds = int.Parse(retryAfter)
        });

        await context.HttpContext.Response.WriteAsync(response, cancellationToken);
    };
});

// ─── Helper: Convert postgres:// URI to Npgsql key-value format ───
static string? ParsePgConnectionString(string? connStr)
{
    if (string.IsNullOrWhiteSpace(connStr)) return null;

    connStr = connStr.Trim();

    // postgres://user:password@host:port/database?options...
    if (connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(connStr);
            var host = uri.Host;
            var port = uri.IsDefaultPort ? "5432" : uri.Port.ToString();
            var db = uri.AbsolutePath.TrimStart('/').Split('?')[0];
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var pass = userInfo.Length > 1 ? userInfo[1] : "";

            var query = uri.Query.TrimStart('?');
            var extra = string.IsNullOrEmpty(query) ? "" : $";{query.Replace("&", ";")}";

            return $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true{extra}";
        }
        catch { return null; } // URI parse failed — can't connect
    }

    return connStr; // already key-value format
}

// Get connection string from appsettings.json or Render PostgreSQL env vars
var connectionString = builder.Configuration.GetConnectionString("dbFoodyEntities");
connectionString = ParsePgConnectionString(connectionString);

// If no connection string in appsettings, build from Render PostgreSQL environment variables
if (string.IsNullOrEmpty(connectionString))
{
    // Try Render's DATABASE_URL first (full connection URL provided by Render)
    var pgUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    pgUrl = ParsePgConnectionString(pgUrl);
    if (!string.IsNullOrEmpty(pgUrl))
    {
        connectionString = pgUrl;
    }
    else
    {
        // Fallback: build from individual PostgreSQL environment variables
        var pgHost     = Environment.GetEnvironmentVariable("PGHOST")     ?? "localhost";
        var pgPort     = Environment.GetEnvironmentVariable("PGPORT")     ?? "5432";
        var pgUser     = Environment.GetEnvironmentVariable("PGUSER")     ?? "postgres";
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "";
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE") ?? "dbFoody";

        connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
    }
}

// ponytail: log host only, never the full connection string (password leak risk)
Log.Information("PostgreSQL connection configured");

// Add Entity Framework Core (PostgreSQL)
builder.Services.AddDbContext<dbFoodyEntities>(options =>
    options.UseNpgsql(connectionString));

// Add HttpContextAccessor for session access
builder.Services.AddHttpContextAccessor();

// ─── Mục 5: Data Protection — lưu khóa mã hóa Cookie bền vững ───
// Tránh mất khóa khi container restart → user bị đăng xuất hàng loạt
// Trên Render: set DATA_PROTECTION_KEY_DIR trỏ đến thư mục persistent (VD: /data/dpk/)
try
{
    var keyRingPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEY_DIR")
        ?? Path.Combine(Directory.GetCurrentDirectory(), "dpk");
    Directory.CreateDirectory(keyRingPath);

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
        .SetApplicationName("FastShip")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

    Log.Information("Data Protection configured (keys dir: {KeyDir})", keyRingPath);
}
catch (Exception ex)
{
    Log.Warning(ex, "Could not configure Data Protection — using ephemeral keys (users may be logged out on restart)");
}

// ─── Mục 5: Cookie Authentication đã cấu hình SlidingExpiration=true, ExpireTimeSpan=30 ngày ───
// Session idle timeout = 1 ngày, đảm bảo người dùng không bị mất session khi reload

// Register Services
builder.Services.AddScoped<ShipFood.Services.RecommendationService>();
builder.Services.AddScoped<ShipFood.Services.VoucherService>();
builder.Services.AddScoped<ShipFood.Services.EDeliveryService>();
// ponytail: AutoPreparingService da xoa — restaurant phai tu xac nhan mon an
// Auto-cancel pending orders sau 15 phút
builder.Services.AddHostedService<ShipFood.Services.AutoCancelPendingOrdersService>();
// ponytail: AddSingleton để tránh mất/gãy API key do SignalR ChatHub tạo lại service liên tục
builder.Services.AddSingleton<ShipFood.Services.GeminiService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    // Ưu tiên đọc từ Environment Variable trước, fallback xuống appsettings.json
    var apiKey = Environment.GetEnvironmentVariable("Gemini__ApiKey") ?? configuration["Gemini:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
        Log.Information("GeminiService initialized as Singleton (key source: {Source})",
            Environment.GetEnvironmentVariable("Gemini__ApiKey") != null ? "env var" : "appsettings");
    return new ShipFood.Services.GeminiService(apiKey);
});

// Register ViewComponents
builder.Services.AddScoped<ShipFood.ViewComponents.FilterBarViewComponent>();

// Register MoMo Payment Service
builder.Services.AddHttpClient<ShipFood.Services.MoMoService>();

// Add Antiforgery (hỗ trợ AJAX header token cho Mock Payment)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// ─── Task 4: CORS Policy — restrict to official domain ───
var allowedOrigins = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
    ?? builder.Configuration["Cors:AllowedOrigins"]
    ?? "https://fastship-web.onrender.com")
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (builder.Environment.IsDevelopment())
{
    // Dev: allow localhost for convenience, but keep production domain too
    allowedOrigins = allowedOrigins
        .Concat(new[] { "http://localhost:3000", "http://localhost:5000", "http://localhost:8080" })
        .Distinct()
        .ToArray();
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

});

Log.Information("CORS configured for origins: {Origins}", string.Join(", ", allowedOrigins));

// Add Authentication (Cookie + optional Google OAuth)
var authBuilder = builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Only add Google OAuth if credentials are configured (prevents crash when not set)
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;

        // Luôn hiển thị cửa sổ "Chọn tài khoản" của Google,
        // kể cả khi chỉ có 1 tài khoản đang đăng nhập hoặc đã cấp quyền trước đó
        googleOptions.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                return Task.CompletedTask;
            }
        };
    });
}

// ─── Facebook OAuth ───
var fbAppId = builder.Configuration["Authentication:Facebook:AppId"];
var fbAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrEmpty(fbAppId) && !string.IsNullOrEmpty(fbAppSecret))
{
    authBuilder.AddFacebook(fbOptions =>
    {
        fbOptions.AppId = fbAppId;
        fbOptions.AppSecret = fbAppSecret;
        fbOptions.Fields.Add("email");
        fbOptions.Fields.Add("name");
        fbOptions.Scope.Add("email");
        fbOptions.Scope.Add("public_profile");
        fbOptions.SaveTokens = true;
    });
}

// ─── HTTPS termination (Render) — ForwardedHeaders middleware ───
// ponytail: Render proxy chạy HTTPS bên ngoài, gửi HTTP vào app bên trong.
// Cấu hình ForwardedHeaders để đọc X-Forwarded-Proto và X-Forwarded-For headers,
// giúp Url.Action() sinh URL redirect OAuth dạng https:// (Facebook & Google yêu cầu)
// Clear KnownNetworks/KnownProxies vì Render proxy không có IP cố định
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// ✅ UseForwardedHeaders() — phải là middleware ĐẦU TIÊN, trước mọi thứ
app.UseForwardedHeaders();

// ─── Task 1b: EF Core Migrations (replacing EnsureCreated) ───
// Auto-create database tables on first run (PostgreSQL)
// Wrapped in try-catch so the app starts even if PostgreSQL is not yet available
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<dbFoodyEntities>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Step 1: Try MigrateAsync() first (for production with migration history)
        bool migrationApplied = false;
        try
        {
            // Check if __EFMigrationsHistory table exists → use Migrations
            var hasMigrationTable = db.Database.GetAppliedMigrations().Any();
            if (hasMigrationTable)
            {
                await db.Database.MigrateAsync();
                migrationApplied = true;
                logger.LogInformation("EF Core Migrations applied successfully");
            }
        }
        catch { /* No migration table yet, fall through to EnsureCreated */ }

        // Step 2: Fallback to EnsureCreated() if no migration system
        if (!migrationApplied)
        {
            var created = db.Database.EnsureCreated();
            if (created)
            {
                logger.LogInformation("Database created via EnsureCreated()");
            }
            else
            {
                logger.LogInformation("Database already exists (EnsureCreated)");
            }

            // Step 3: Seed data if DB is empty (PostgreSQL)
            if (!db.tbUsers.Any())
            {
                var sqlPath = Path.Combine(app.Environment.ContentRootPath, "seed.sql");
                if (!File.Exists(sqlPath))
                    sqlPath = Path.Combine(app.Environment.ContentRootPath, "..", "seed.sql");

                if (File.Exists(sqlPath))
                {
                    logger.LogInformation("Seeding database from {SqlPath}", sqlPath);
                    var sql = File.ReadAllText(sqlPath).Replace("\r\n", "\n");
                    var statements = sql.Split(new[] { "\nGO\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var stmt in statements)
                    {
                        var trimmed = stmt.Trim();
                        if (trimmed.Length > 0 && !trimmed.StartsWith("DROP") && !trimmed.StartsWith("CREATE") && !trimmed.StartsWith("SET"))
                        {
                            try { db.Database.ExecuteSqlRaw(trimmed); }
                            catch (Exception seedEx) { logger.LogWarning("Seed statement skipped: {Error}", seedEx.Message); }
                        }
                    }
                    logger.LogInformation("Database seeding completed");
                }
            }
        }

        // ═══ Auto-migrate new columns (chạy trên MỌI startup, ko chỉ DB rỗng) ═══
        // ponytail: thêm cột mới vào DB bằng ALTER TABLE ADD COLUMN IF NOT EXISTS.
        // Khi thêm cột mới vào C# model, thêm ALTER ở đây để tự động migration khi deploy.
        var alterStatements = new[]
        {
            @"ALTER TABLE ""tbDanhMuc"" ADD COLUMN IF NOT EXISTS ""icon"" VARCHAR(50);",
            @"ALTER TABLE ""tbKhachHang"" ADD COLUMN IF NOT EXISTS ""hinhanh"" VARCHAR(500);",
            @"ALTER TABLE ""tbAdmin"" ADD COLUMN IF NOT EXISTS ""hinhanh"" VARCHAR(500);",
        };
        foreach (var alter in alterStatements)
        {
            try { db.Database.ExecuteSqlRaw(alter); }
            catch (Exception alterEx) { logger.LogWarning("ALTER TABLE skipped: {Error}", alterEx.Message); }
        }
        logger.LogInformation("Auto-migration: ALTER TABLE columns checked");

        // ═══ Fix PostgreSQL sequence out-of-sync (prevent PK violation on signup) ═══
        // ponytail: Sau khi seed v?i userid explicit (1-18), sequence v?n ? 1 → signup b? l?i 23505
        // Reset sequence = max(userid) + 1 m?i l?n start (safe, nhanh)
        try
        {
            db.Database.ExecuteSqlRaw(@"SELECT setval('""tbUser_userid_seq""', COALESCE((SELECT MAX(""userid"") FROM ""tbUser""), 0) + 1, false);");
            logger.LogInformation("PostgreSQL sequence tbUser_userid_seq synced to max(userid) + 1");
        }
        catch (Exception seqEx)
        {
            logger.LogWarning(seqEx, "Could not reset tbUser sequence — may cause PK violation on signup");
        }
    }
}
catch (Exception ex)
{
    // App still starts - database can be initialized later
    Console.WriteLine($"[WARN] Database initialization failed: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Global exception handler bắt mọi lỗi chưa được xử lý - trả về JSON cho AJAX, HTML cho normal requests
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception at {Path} | Trace: {TraceId}", context.Request.Path, traceId);

        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = $"Lỗi máy chủ nội bộ. Vui lòng thử lại sau. (Mã lỗi: {traceId})",
                traceId = traceId
            });
            await context.Response.WriteAsync(errorResponse);
        }
        else
        {
            // Hiển thị trace ID cho user để dễ dàng tra cứu lỗi
            context.Response.Redirect($"/Home/Error?traceId={traceId}");
        }
    }
});

// ═══ Health check details (9.8) — verify DB + disk ═══
app.MapGet("/health", async (HttpContext context) =>
{
    var result = new Dictionary<string, object> { ["status"] = "healthy", ["timestamp"] = DateTime.UtcNow };
    try
    {
        using var scope = context.RequestServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<dbFoodyEntities>();
        await db.Database.CanConnectAsync();
        result["database"] = "connected";
    }
    catch (Exception ex)
    {
        result["database"] = $"error: {ex.Message}";
        result["status"] = "degraded";
    }
    try
    {
        var root = Directory.GetCurrentDirectory();
        var drive = new System.IO.DriveInfo(root);
        result["disk_free_mb"] = Math.Round(drive.AvailableFreeSpace / 1024.0 / 1024.0);
    }
    catch { result["disk_free_mb"] = "unknown"; }
    return Results.Json(result);
});

// Ensure UTF-8 charset for all HTML responses (fixes Vietnamese character encoding)
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var ct = context.Response.ContentType;
        if (ct != null &&
            ct.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) &&
            !ct.Contains("charset", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType = "text/html; charset=utf-8";
        }
        return Task.CompletedTask;
    });
    await next();
});

// Ép HTTPS redirect — chỉ kích hoạt ở Development (Render đã xử lý SSL ở proxy)
// Trong production, Render proxy đã terminate SSL, không cần redirect nữa
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ═══ Response compression (Gzip/Brotli) — gi?m ~70% bandwidth ═══
app.UseResponseCompression();

// ═══ Cache-Control + ETag cho static assets (7.3) ═══
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // ponytail: file có hash (VD: main.v123.js) → cache 1 n?m
        var dotCount = ctx.File.Name.Count(c => c == '.');
        var cacheMaxAge = dotCount >= 2 ? TimeSpan.FromDays(365) : TimeSpan.FromDays(1);
        ctx.Context.Response.Headers.CacheControl = $"public, max-age={(int)cacheMaxAge.TotalSeconds}";
        ctx.Context.Response.Headers.ETag = $"\"{ctx.File.LastModified:yyyyMMddHHmmss}\"";
    }
});

app.UseRouting();

app.UseCors();

// Rate Limiting middleware (Task 1c)
app.UseRateLimiter();

// Session phải đặt TRƯỚC Authentication để cookie session hoạt động đúng với Google OAuth
app.UseSession();

// ─── Authentication TRƯỚC RoleGuard — RoleGuard cần context.User.IsAuthenticated ───
// Nếu RoleGuard chạy trước Authentication → context.User luôn unauthenticated → 401 cho mọi AJAX
app.UseAuthentication();

// ─── Phase 3: RoleGuard Middleware (must be AFTER UseSession + UseAuthentication) ───
app.UseMiddleware<ShipFood.Middleware.RoleGuardMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<ShipFood.Hubs.Chats>("/nhantin");

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
