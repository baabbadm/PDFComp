using FileCompressor.Services;
using FileCompressor.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ تحميل إعدادات الاتصال
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ✅ تسجيل DbContext مع MySQL/MariaDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6))));

// ✅ تسجيل الخدمات
builder.Services.AddControllers(); 
builder.Services.AddScoped<PdfCompressor>();

// ✅ تفعيل Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ في بيئة التطوير، فعّل Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ دعم الملفات الثابتة (wwwroot)
app.UseStaticFiles();

// ✅ توجيه الطلبات
app.UseRouting();
app.MapControllers();

// ✅ توجيه أي صفحة غير معرفة إلى auth.html (مثل تسجيل الدخول)
app.MapFallbackToFile("register.html");


app.Run();
