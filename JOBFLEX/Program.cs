using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình Session
builder.Services.AddDistributedMemoryCache(); // Đảm bảo sử dụng cache để lưu trữ session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session (tùy chỉnh)
    options.Cookie.HttpOnly = true; // Đảm bảo cookie chỉ có thể được truy cập qua HTTP
    options.Cookie.IsEssential = true; // Đảm bảo cookie luôn được gửi
});

// Thêm DbContext với MySQL và logging
builder.Services.AddDbContext<HeThongTimViecContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
    .LogTo(Console.WriteLine, LogLevel.Information)); // Ghi log để debug

var app = builder.Build();

// Sử dụng Session trong middleware
app.UseSession(); // Đây là phần thêm vào để đảm bảo session hoạt động

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Đảm bảo cơ sở dữ liệu được tạo khi khởi động (tùy chọn)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HeThongTimViecContext>();
    dbContext.Database.EnsureCreated(); // Tạo DB nếu chưa tồn tại (dùng khi không dùng Migration)
}

app.Run();