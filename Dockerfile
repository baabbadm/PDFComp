# المرحلة الأساسية (تشغيل)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# تثبيت Ghostscript داخل الصورة النهائية
RUN apt-get update && apt-get install -y ghostscript

# مرحلة البناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# الصورة النهائية للتشغيل
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FileCompressor.dll"]
