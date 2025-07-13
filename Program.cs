using FileCompressor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ✅ إعداد الجلسات
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ إضافة الخدمات المطلوبة
builder.Services.AddControllers();
builder.Services.AddScoped<PdfCompressor>();

// ✅ إعداد Swagger (اختياري)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ تفعيل Swagger في بيئة التطوير
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ ملفات ثابتة + الجلسة
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ✅ مسارات الـ Controllers
app.MapControllers();

// ✅ صفحة auth.html للمستخدمين غير المسجلين
app.MapFallbackToFile("auth.html");

// ✅ شغّل التطبيق
app.Run();
