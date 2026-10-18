# TicketApp 🎟️

[![Build and Verify](https://github.com/muyasemih/TicketApp/actions/workflows/ci.yml/badge.svg)](https://github.com/muyasemih/TicketApp/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)
![Angular](https://img.shields.io/badge/Angular-22-dd0031.svg)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red.svg)
![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)

TicketApp; etkinlik keşfi, salon krokisi üzerinden dinamik koltuk ve ayakta alan seçimi, rezervasyon, ödeme simülasyonu ve bilet yönetimini sağlayan tam kapsamlı (Full-Stack) modern bir web uygulamasıdır.

Kullanıcılar etkinlikleri filtreleyip inceleyebilir, oturma planı üzerinden diledikleri koltukları seçerek bilet satın alabilir, profil bilgilerini ve geçmiş siparişlerini yönetebilir. Yetkili kullanıcılar (Admin) ise salon, blok ve etkinlik oluşturup yönetebilir.

---

## 🚀 Kullanılan Teknolojiler

### Backend
- **C# & .NET 10** (ASP.NET Core Web API)
- **Entity Framework Core** (Code-First yaklaşımı, `AsNoTracking` performans optimizasyonu)
- **Microsoft SQL Server 2022** (İlişkisel veritabanı)
- **JWT (JSON Web Token)** Kimlik Doğrulama & Rol Tabanlı Yetkilendirme (Admin / User)
- **BCrypt.Net** (Güvenli şifre tuzlama ve hashleme)
- **Scalar & OpenAPI 3.1** (Modern API dokümantasyonu)

### Frontend
- **Angular 22** (Standalone Components, Signals & Reaktif Mimari)
- **TypeScript & CSS3**
- **Angular Router & Guards** (`authGuard`, `adminGuard`)
- **HTTP Interceptor** (Otomatik Bearer Token enjeksiyonu)
- **Jasmine & Karma** (Kapsamlı birim testleri)
- **Nginx** (Production web sunucusu & ters proxy)

### DevOps & Altyapı
- **Docker & Docker Compose** (Çoklu konteyner mimarisi: API, Nginx/Frontend, SQL Server)
- **Nginx Reverse Proxy** (`/api` trafiğini iç ağda API servisine yönlendirme)
- **GitHub Actions** (Otomatik CI derleme, doğrulama ve Angular birim test hattı)

---

## 🎯 Temel Yetenekler & Özellikler

- **Dinamik Koltuk & Ayakta Alan Yönetimi:** Salon blokları üzerinden gerçek zamanlı koltuk seçimi; ayakta bloklar için sanal kapasite modeliyle eşzamanlı biletleme.
- **Eşzamanlılık Koruması (Concurrency):** `RowVersion` mekanizması ile aynı koltuğun birden fazla kullanıcı tarafından eşzamanlı satın alınması engellenir.
- **Rezervasyon & Zaman Aşımı:** Seçilen koltuklar 15 dakika boyunca kullanıcı adına kilitlenir; süre dolduğunda otomatik olarak boşa çıkar.
- **Ödeme & Bilet İptal / İade:** Kart doğrulama ve iade simülasyonu. Etkinlik başlamadan önce kullanıcılar biletlerini iptal edebilir ve koltuk tekrar satışa sunulur.
- **Öğrenci İndirimi:** Öğrenci hesapları için sunucu tarafında güvenli şekilde hesaplanan %10 indirim.
- **Rol Tabanlı Erişim:** Admin ve Standart Kullanıcı rolleriyle ayrıştırılmış işlem yetkileri.
- **Arama & Performans:** 300ms debounce ile arama kutusu optimizasyonu, sunucu taraflı sayfalama (pagination) ve UTC saat dilimi desteği.

---

## 📡 API Uç Noktaları

### 🔐 Kimlik Doğrulama & Profil (`/api/users`)
| HTTP Metot | Endpoint | Yetki | Açıklama |
|---|---|---|---|
| `POST` | `/api/users` | Public | Yeni kullanıcı kaydı oluşturur |
| `POST` | `/api/users/login` | Public | Giriş yapar ve JWT Bearer Token döner |
| `GET` | `/api/users/me` | User / Admin | Giriş yapmış kullanıcının profil bilgilerini döner |
| `PUT` | `/api/users/me` | User / Admin | Profil bilgilerini ve şifreyi günceller |

### 🎪 Etkinlikler (`/api/events`)
| HTTP Metot | Endpoint | Yetki | Açıklama |
|---|---|---|---|
| `GET` | `/api/events` | Public | Aktif etkinlikleri listeler (sayfalama ve arama destekli) |
| `GET` | `/api/events/{id}` | Public | Belirtilen ID'li etkinliğin detaylarını getirir |
| `GET` | `/api/events/{id}/seats`| Public | Etkinliğe ait koltuk krokisi ve durumlarını getirir |
| `POST` | `/api/events` | Admin | Yeni etkinlik oluşturur |
| `PUT` | `/api/events/{id}` | Admin | Etkinlik bilgilerini günceller |
| `DELETE` | `/api/events/{id}` | Admin | Etkinliği siler |

### 🏛️ Salonlar (`/api/venues`)
| HTTP Metot | Endpoint | Yetki | Açıklama |
|---|---|---|---|
| `GET` | `/api/venues` | Public | Tüm salonları listeler |
| `GET` | `/api/venues/{id}` | Public | Salon ve blok detaylarını getirir |
| `POST` | `/api/venues` | Admin | Yeni salon ve blok şeması oluşturur |
| `PUT` | `/api/venues/{id}` | Admin | Salon bilgilerini günceller |
| `DELETE` | `/api/venues/{id}` | Admin | Salonu siler (etkinliği varsa korumalıdır) |

### 🎟️ Koltuk, Sipariş & Biletleme (`/api/orders`)
| HTTP Metot | Endpoint | Yetki | Açıklama |
|---|---|---|---|
| `GET` | `/api/orders` | User / Admin | Kullanıcının geçmiş sipariş ve biletlerini listeler |
| `POST` | `/api/orders` | User / Admin | Seçili koltuklar için bilet siparişi oluşturur |
| `POST` | `/api/orders/tickets/{id}/cancel` | User / Admin | Bileti iptal eder, iade başlatır ve koltuğu serbest bırakır |

---

## 🛠️ Kurulum ve Çalıştırma

### Gereksinimler
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)
- *(.NET 10 SDK & Node.js 22 - yalnızca yerel geliştirmede gereklidir)*

---

### Yöntem 1: Docker Compose ile Tek Tuşla Çalıştırma (Önerilen) 🐳

Projeyi tüm bağımlılıklarıyla (SQL Server, .NET API ve Angular Frontend) ayağa kaldırmak için:

1. **Depoyu klonlayın:**
   ```bash
   git clone https://github.com/muyasemih/TicketApp.git
   cd TicketApp
   ```

2. **Konteynerleri derleyin ve başlatın:**
   ```bash
   docker compose up --build
   ```

3. **Uygulamaya erişin:**
   - **Frontend (Web UI):** [http://localhost:4200](http://localhost:4200)
   - **Backend API & Scalar Dokümantasyonu:** [http://localhost:5040/scalar/v1](http://localhost:5040/scalar/v1)
   - **SQL Server:** `localhost:1433` *(Veritabanı: TicketAppDb, Şifre: Staj1234StrongPass)*

---

### Yöntem 2: Lokal Geliştirme Ortamı

Yalnızca veritabanını Docker üzerinde başlatıp geliştirme yapmak isterseniz:

1. **SQL Server'ı başlatın:**
   ```bash
   docker compose up -d sqlserver
   ```

2. **Veritabanı migrasyonlarını uygulayın ve API'yi çalıştırın:**
   ```bash
   dotnet restore
   dotnet ef database update
   dotnet run --project TicketApp.csproj
   ```

3. **Frontend'i çalıştırın:**
   ```bash
   cd frontend
   npm install
   npm start
   ```
   Tarayıcınızda [http://localhost:4200](http://localhost:4200) adresine gidin.

---

## 🧪 Testler

Frontend birim testlerini çalıştırmak için:

```bash
cd frontend
npm test -- --watch=false
```
> Tüm test suite'leri (`login`, `register`, `events`, `seats`, `orders`, `admin`, `event-detail`) Jasmine ve Karma ile çalıştırılır ve GitHub Actions üzerinde otomatik doğrulanır.

---

## 🔒 Güvenlik Standartları

- **Şifre Güvenliği:** Kullanıcı şifreleri veritabanında düz metin olarak değil, BCrypt algoritmasıyla tuzlanarak (salt) saklanır.
- **Yetkilendirme:** API uç noktaları `[Authorize]` öznitelikleri ve JWT Bearer token doğrulaması ile korunmaktadır.
- **Frontend Koruması:** Admin ve korumalı sayfalara doğrudan erişim `authGuard` ve `adminGuard` ile engellenir.
- **Veri Bütünlüğü:** Bilet satılmış etkinliklerin salonunun değiştirilmesi veya etkinliği olan salonların silinmesi engellenerek ilişkisel veri bütünlüğü garanti altına alınmıştır.