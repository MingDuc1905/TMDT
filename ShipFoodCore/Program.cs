using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using ShipFood.Models;

var builder = WebApplication.CreateBuilder(args);

// In production (Railway), use PORT env var dynamically
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

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

    // Login: 5 attempts in 5 minutes (sliding window)
    options.AddSlidingWindowLimiter("login-policy", opt =>
    {
        opt.PermitLimit = 5;
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

// Get connection string from Railway env vars or appsettings.json
var connectionString = builder.Configuration.GetConnectionString("dbFoodyEntities");

// If no connection string in appsettings, build from Railway MySQL environment variables
if (string.IsNullOrEmpty(connectionString))
{
    // Try MYSQL_URL first (full connection URL provided by Railway)
    var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
    if (!string.IsNullOrEmpty(mysqlUrl))
    {
        connectionString = mysqlUrl;
    }
    else
    {
        // Fallback: build from individual Railway MySQL environment variables
        var mysqlHost     = Environment.GetEnvironmentVariable("MYSQLHOST")     ?? "localhost";
        var mysqlPort     = Environment.GetEnvironmentVariable("MYSQLPORT")     ?? "3306";
        var mysqlUser     = Environment.GetEnvironmentVariable("MYSQLUSER")     ?? "root";
        var mysqlPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD") ?? "";
        var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQLDATABASE") ?? "dbFoody";

        connectionString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};User={mysqlUser};Password={mysqlPassword};SslMode=Preferred;";
    }
}

// Add Entity Framework Core (MySQL)
builder.Services.AddDbContext<dbFoodyEntities>(options =>
    options.UseMySql(
        connectionString,
        new MariaDbServerVersion(new Version(10, 6))
    ));

// Add HttpContextAccessor for session access
builder.Services.AddHttpContextAccessor();

// Register Services
builder.Services.AddScoped<ShipFood.Services.RecommendationService>();
builder.Services.AddHostedService<ShipFood.Services.AutoPreparingService>(); // Task 3b
builder.Services.AddScoped<ShipFood.Services.GeminiService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiKey = configuration["Gemini:ApiKey"];
    return new ShipFood.Services.GeminiService(apiKey);
});

// Add Antiforgery (hỗ trợ AJAX header token cho Mock Payment)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



// Add Authentication (Cookie + optional Google OAuth)
var authBuilder = builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
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
    });
}

var app = builder.Build();

// ─── Task 1b: EF Core Migrations (replacing EnsureCreated) ───
// Auto-create database tables on first run (MySQL)
// Wrapped in try-catch so the app starts even if MySQL is not yet available
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

            // Step 3: Seed data if DB is empty
            if (!db.tbUsers.Any())
            {
                string sqlPath = Path.Combine(app.Environment.ContentRootPath, "seed_mysql.sql");
                if (!File.Exists(sqlPath))
                    sqlPath = Path.Combine(app.Environment.ContentRootPath, "..", "seed_mysql.sql");

                if (File.Exists(sqlPath))
                {
                    logger.LogInformation("Seeding database from {SqlPath}", sqlPath);
                    var sql = File.ReadAllText(sqlPath).Replace("\r\n", "\n");
                    var statements = sql.Split(new[] { "\nGO\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var stmt in statements)
                    {
                        var trimmed = stmt.Trim();
                        if (trimmed.Length > 0 && !trimmed.StartsWith("--") && !trimmed.StartsWith("DROP") && !trimmed.StartsWith("CREATE"))
                        {
                            try { db.Database.ExecuteSqlRaw(trimmed); } catch { }
                        }
                    }
                    logger.LogInformation("Database seeding completed");
                }
            }
        }

        // BCrypt fix: mở rộng cột pwd từ VARCHAR(50) → VARCHAR(255)
        try
        {
            db.Database.ExecuteSqlRaw("ALTER TABLE tbUser MODIFY COLUMN pwd VARCHAR(255) NOT NULL;");
            logger.LogInformation("Column tbUser.pwd expanded to VARCHAR(255) for BCrypt compatibility");
        }
        catch (Exception ex)
        {
            logger.LogWarning("Could not alter tbUser.pwd column: {Error}", ex.Message);
        }

        // Task 2c: Thêm cột conhang cho tbMonAn (nếu chưa có)
        try
        {
            db.Database.ExecuteSqlRaw(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
                    WHERE TABLE_NAME = 'tbMonAn' AND COLUMN_NAME = 'conhang' AND TABLE_SCHEMA = DATABASE());
                SET @sql := IF(@exist = 0,
                    'ALTER TABLE tbMonAn ADD COLUMN conhang BIT DEFAULT 1 AFTER madanhmuc',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;");
            logger.LogInformation("Column tbMonAn.conhang ensured (BIT DEFAULT 1)");
        }
        catch (Exception ex)
        {
            logger.LogWarning("Could not add tbMonAn.conhang column: {Error}", ex.Message);
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
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);

        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"success\":false,\"message\":\"Lỗi máy chủ nội bộ. Vui lòng thử lại sau.\"}");
        }
        else
        {
            context.Response.Redirect("/Home/Error");
        }
    }
});

// Dedicated healthcheck endpoint (no database dependency, always returns 200)
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Ensure UTF-8 charset for all HTML responses (fixes Vietnamese character encoding on Railway)
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

// Railway HTTPS termination — đặt sớm để xử lý X-Forwarded-Proto header
// Railway's reverse proxy chạy HTTPS bên ngoài, gửi HTTP vào app bên trong.
// Middleware này đọc header X-Forwarded-Proto và set Scheme = https,
// giúp Url.Action() sinh URL redirect OAuth dạng https:// (Google yêu cầu)
app.Use(async (context, next) =>
{
    var forwardedProto = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    if (forwardedProto?.Equals("https", StringComparison.OrdinalIgnoreCase) == true)
    {
        context.Request.Scheme = "https";
    }
    await next();
});

// Ép HTTPS redirect — khi request đến bằng HTTP, tự động redirect sang HTTPS
// (đặt trước UseAuthentication để URL callback OAuth dùng https://)
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

// Rate Limiting middleware (Task 1c)
app.UseRateLimiter();

// Session phải đặt TRƯỚC Authentication để cookie session hoạt động đúng với Google OAuth
app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<ShipFood.Hubs.Chats>("/nhantin");

app.Run();
