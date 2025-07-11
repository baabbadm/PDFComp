using FileCompressor.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ تسجيل الخدمات المطلوبة
builder.Services.AddControllers(); // ← لدعم Controllers مثل CompressController
builder.Services.AddScoped<PdfCompressor>();

// ✅ تفعيل Swagger (اختياري للتوثيق)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ في بيئة التطوير، فعّل Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ دعم HTTPS وملفات wwwroot
//app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ تفعيل الـ Routing وتسجيل الـ Controllers
app.UseRouting();
app.MapControllers();

// ✅ تشغيل التطبيق
app.MapFallbackToFile("index.html");

app.Run();
