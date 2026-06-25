using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

var builder = WebApplication.CreateBuilder(args);

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

// Add Entity Framework Core
builder.Services.AddDbContext<dbFoodyEntities>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("dbFoodyEntities"),
        sqlOptions => sqlOptions.UseNetTopologySuite()
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

// Add Authentication (Cookie + Google OAuth)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });

var app = builder.Build();

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

// app.UseHttpsRedirection(); // Tắt HTTPS redirect vì app dev chỉ chạy HTTP port 5000 - Google OAuth callback cần URI khớp
app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<ShipFood.Hubs.Chats>("/nhantin");

app.Run();
