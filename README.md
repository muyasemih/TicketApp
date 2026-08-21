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

Projede Repository Pattern kullanılmıştır.

Controller → Repository → AppDbContext → Entity Framework Core → SQL Server

## API Endpointleri

| HTTP Method | Endpoint | Açıklama |
|---|---|---|
| GET | `/api/events` | Tüm etkinlikleri getirir |
| GET | `/api/events/{id}` | ID'ye göre etkinlik getirir |
| POST | `/api/events` | Yeni etkinlik oluşturur |
| PUT | `/api/events/{id}` | Etkinlik bilgilerini günceller |
| DELETE | `/api/events/{id}` | Etkinliği siler |

## Veritabanı

Proje SQL Server kullanmaktadır. SQL Server Docker container üzerinden çalıştırılmaktadır.

Veritabanı işlemleri Entity Framework Core ve AppDbContext üzerinden gerçekleştirilmektedir.

## Güvenlik

Veritabanı bağlantı bilgileri ve şifre gibi hassas bilgiler User Secrets kullanılarak proje kodundan ayrı tutulmaktadır.