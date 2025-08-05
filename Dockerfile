# Geliştirme aşaması için .NET SDK imajını temel al
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Çözüm dosyasını kopyala
COPY backend.sln ./

# Tüm proje dosyalarını kopyala
COPY . ./

# Bağımlılıkları yükle (çözüm dosyasını kullanarak)
RUN dotnet restore

# Ana proje dizinine geç
WORKDIR /app/backend

# Uygulamayı yayınla
RUN dotnet publish -c Release -o out

# Çalıştırma aşaması için ASP.NET Core runtime imajını temel al (web uygulamaları için)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/backend/out ./
ENTRYPOINT ["dotnet", "backend.dll"]