using Microsoft.EntityFrameworkCore;
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

// Add Session support (replacing HttpSession in old project)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

// Auto-create database tables on first run (MySQL)
// Wrapped in try-catch so the app starts even if MySQL is not yet available
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<dbFoodyEntities>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var created = db.Database.EnsureCreated();

        // If database was just created, seed initial data from seed_mysql.sql            if (created && !db.tbUsers.Any())
            {
                // Try to find seed_mysql.sql in ContentRootPath (Docker) or parent (local dev)
                string sqlPath = Path.Combine(app.Environment.ContentRootPath, "seed_mysql.sql");
                if (!File.Exists(sqlPath))
                {
                    sqlPath = Path.Combine(app.Environment.ContentRootPath, "..", "seed_mysql.sql");
                }

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

            // === BCrypt fix: mở rộng cột pwd từ VARCHAR(50) → VARCHAR(255) ===
            // BCrypt hash dài 60 ký tự, cột cũ 50 ký tự gây lỗi khi SaveChanges() nâng cấp password
            try
            {
                db.Database.ExecuteSqlRaw("ALTER TABLE tbUser MODIFY COLUMN pwd VARCHAR(255) NOT NULL;");
                logger.LogInformation("Column tbUser.pwd expanded to VARCHAR(255) for BCrypt compatibility");
            }
            catch (Exception ex)
            {
                logger.LogWarning("Could not alter tbUser.pwd column: {Error}", ex.Message);
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

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

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
