using JOBFLEX.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext
builder.Services.AddDbContext<JobFlexDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Thêm Session (nếu bạn dùng trong AccountController)
builder.Services.AddSession();

// Thêm dịch vụ MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Kích hoạt Session nếu cần

// Chỉ thêm nếu bạn thực sự dùng Authentication/Authorization
 app.UseAuthentication();
 app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();