using Microsoft.AspNetCore.Mvc;
using System;
using MySql.Data.MySqlClient;

public class DatabaseTestController : Controller
{
    public IActionResult Index()
    {
        string connectionString = "Server=localhost;Database=HETHONGTIMVIEC;User Id=root;Password=Thanh7778;";

        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open(); // Mở kết nối MySQL
                return Content("✅ Kết nối MySQL thành công!");
            }
        }
        catch (Exception ex)
        {
            return Content($"❌ Lỗi kết nối: {ex.Message}");
        }
    }
}