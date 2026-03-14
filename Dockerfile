# مرحلة البناء (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات الـ csproj بالأسماء المطابقة للصورة بالظبط
COPY ["SolarSystem.WebApi/SolarSystem.WebApi.csproj", "SolarSystem.WebApi/"]
COPY ["SolarSystem.DataAccess1/SolarSystem.DataAccess1.csproj", "SolarSystem.DataAccess1/"]
COPY ["SolarSystem.Models1/SolarSystem.Models1.csproj", "SolarSystem.Models1/"]

# عمل Restore
RUN dotnet restore "SolarSystem.WebApi/SolarSystem.WebApi.csproj"

# نسخ باقي الملفات
COPY . .
WORKDIR "/src/SolarSystem.WebApi"

# بناء المشروع
RUN dotnet build "SolarSystem.WebApi.csproj" -c Release -o /app/build

# مرحلة النشر (Publish)
FROM build AS publish
RUN dotnet publish "SolarSystem.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# المرحلة النهائية (Run)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

# تشغيل الـ DLL النهائي
ENTRYPOINT ["dotnet", "SolarSystem.WebApi.dll"]