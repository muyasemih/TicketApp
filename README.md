# TicketApp

## Proje Hakkında

TicketApp, etkinliklerin yönetilmesini sağlayan bir ASP.NET Core Web API projesidir.

Uygulama üzerinden etkinlikler oluşturulabilir, listelenebilir, ID üzerinden görüntülenebilir, güncellenebilir ve silinebilir.

## Kullanılan Teknolojiler

- C#
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Docker
- Git / GitHub

## Proje Mimarisi

Controller → Repository → AppDbContext → Entity Framework Core → SQL Server

## API Endpointleri

| HTTP Method | Endpoint | Açıklama |
|---|---|---|
| GET | `/api/events` | Tüm etkinlikleri getirir |
| GET | `/api/events/{id}` | ID'ye göre etkinlik getirir |
| POST | `/api/events` | Yeni etkinlik oluşturur |
| PUT | `/api/events/{id}` | Etkinlik bilgilerini günceller |
| DELETE | `/api/events/{id}` | Etkinliği siler |

## Kurulum

### Gereksinimler

- .NET 10 SDK
- Docker Desktop
- Git

### Kurulum Adımları

git clone https://github.com/muyasemih/TicketApp.git
cd TicketApp

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YOUR_PASSWORD" -p 1433:1433 --name staj-sql -d mcr.microsoft.com/mssql/server:2022-latest

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=TicketAppDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"

dotnet tool install --global dotnet-ef

dotnet restore
dotnet build
dotnet ef database update
dotnet run

Uygulama çalıştırıldıktan sonra:

http://localhost:5040

adresinden API'ye erişilebilir.

## Veritabanı

SQL Server Docker container üzerinden çalıştırılmaktadır.

Entity Framework Core ve AppDbContext kullanılarak veritabanı işlemleri gerçekleştirilmektedir.

## Güvenlik

Veritabanı bağlantı bilgileri ve şifre gibi hassas bilgiler User Secrets kullanılarak proje kodundan ayrı tutulmaktadır.