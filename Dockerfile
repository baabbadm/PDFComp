# 1. صورة التشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# 2. صورة البناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/FileCompressor   # <-- تأكد أن هذا اسم مجلد المشروع
RUN dotnet publish -c Release -o /app/publish

# 3. مرحلة التشغيل
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FileCompressor.dll"]
