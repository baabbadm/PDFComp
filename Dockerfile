FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# تثبيت ghostscript في نظام لينكس
RUN apt-get update && apt-get install -y ghostscript

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# أمر التشغيل
ENTRYPOINT ["dotnet", "FileCompressor.dll"]
