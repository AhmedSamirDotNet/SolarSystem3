# مرحلة البناء (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات المشروع وعمل Restore (غير الأسامي لأسامي مشاريعك بالظبط)
COPY ["SolarSystem.API/SolarSystem.API.csproj", "SolarSystem.API/"]
COPY ["SolarSystem.DataAccess1/SolarSystem.DataAccess1.csproj", "SolarSystem.DataAccess1/"]
COPY ["SolarSystem.Models1/SolarSystem.Models1.csproj", "SolarSystem.Models1/"]

RUN dotnet restore "SolarSystem.API/SolarSystem.API.csproj"

# نسخ باقي الملفات وبناء المشروع
COPY . .
WORKDIR "/src/SolarSystem.API"
RUN dotnet build "SolarSystem.API.csproj" -c Release -o /app/build

# مرحلة النشر (Publish)
FROM build AS publish
RUN dotnet publish "SolarSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# المرحلة النهائية (Run)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SolarSystem.API.dll"]