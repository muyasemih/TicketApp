TicketApp 🎟️
TicketApp, etkinlik keşfi, dinamik koltuk seçimi ve bilet satın alma süreçlerini yöneten tam kapsamlı (Full-Stack) bir web uygulamasıdır.
Kullanıcılar etkinlikleri listeleyebilir, salon krokisi üzerinden diledikleri koltukları seçerek bilet satın alabilir, profil ve geçmiş siparişlerini yönetebilir. Yetkili kullanıcılar ise Admin paneli üzerinden etkinlik oluşturabilir ve yönetebilir.
🚀 Kullanılan Teknolojiler
Backend
C# & .NET 10 (ASP.NET Core Web API)
Entity Framework Core (Code-First)
Microsoft SQL Server 2022
JWT (JSON Web Token) Kimlik Doğrulama & Rol Tabanlı Yetkilendirme (Admin / User)
BCrypt.Net (Güvenli Şifre Hashleme)
Frontend
Angular (Standalone Components, Signals & Reactive Architecture)
TypeScript & modern CSS
Angular Router & Route Guards (authGuard, adminGuard)
Nginx (Production Frontend Web Server)
DevOps & Altyapı
Docker & Docker Compose (Çoklu konteyner mimarisi: API, Frontend, SQL Server)
GitHub Actions (CI/CD Pipeline)
🎯 Temel Özellikler
Kullanıcı & Profil Yönetimi: Kayıt olma, JWT tabanlı güvenli giriş, şifre değiştirme ve profil bilgilerini güncelleme.
Dinamik Koltuk Seçimi: Salon blokları ve oturma planı üzerinden gerçek zamanlı koltuk seçimi ve biletleme.
Öğrenci İndirimi: Öğrenci hesapları için sunucu tarafında otomatik uygulanan %10 indirim hesaplaması (istemci fiyat manipülasyonlarına karşı korumalı).
Rol Tabanlı Erişim: Admin ve Standart Kullanıcı rolleriyle ayrıştırılmış yetki mekanizması.
Sipariş Geçmişi: Kullanıcıların satın aldıkları biletleri detaylarıyla görüntüleyebilmesi.
📡 API Endpointleri
🔐 Kimlik Doğrulama & Profil (/api/users)
HTTP Method	Endpoint	Yetki	Açıklama
POST	/api/users	Public	Yeni kullanıcı kaydı oluşturur
POST	/api/users/login	Public	Giriş yapar ve JWT Bearer Token döner
GET	/api/users/me	User / Admin	Giriş yapmış kullanıcının profil bilgilerini döner
PUT	/api/users/me	User / Admin	Profil bilgilerini ve şifreyi günceller
🎪 Etkinlikler (/api/events)
HTTP Method	Endpoint	Yetki	Açıklama
GET	/api/events	Public	Tüm aktif etkinlikleri listeler
GET	/api/events/{id}	Public	Belirtilen ID'li etkinliğin detaylarını getirir
POST	/api/events	Admin	Yeni etkinlik oluşturur
PUT	/api/events/{id}	Admin	Etkinlik bilgilerini günceller
DELETE	/api/events/{id}	Admin	Etkinliği siler
🎟️ Koltuk & Biletleme (/api/orders)
HTTP Method	Endpoint	Yetki	Açıklama
GET	/api/orders	User / Admin	Kullanıcının geçmiş biletlerini listeler
POST	/api/orders	User / Admin	Seçili koltuklar için bilet siparişi oluşturur
🛠️ Kurulum ve Çalıştırma
Gereksinimler
Docker Desktop
Git
(.NET 10 SDK & Node.js opsiyonel)
Yöntem 1: Docker Compose ile Tek Adımda Çalıştırma (Önerilen)
Projeyi tüm bağımlılıklarıyla (SQL Server, API ve Angular Frontend) ayağa kaldırmak için:
Depoyu klonlayın:
git clone https://github.com/muyasemih/TicketApp.git
cd TicketApp
Konteynerleri derleyin ve başlatın:
docker compose up -d --build
Uygulamaya erişin:
Frontend (Web UI): http://localhost:4200
Backend API: http://localhost:5040
SQL Server: localhost:1433 (Veritabanı: TicketAppDb)
Yöntem 2: Lokal Geliştirme Ortamı
Yalnızca veritabanını Docker üzerinde başlatın:
docker compose up -d sqlserver
Veritabanı migrasyonlarını uygulayın ve API'yi çalıştırın:
dotnet restore
dotnet ef database update
dotnet run --project TicketApp.csproj
Frontend'i başlatın:
cd frontend
npm install
npm start
🔒 Güvenlik
Şifre Güvenliği: Kullanıcı şifreleri veritabanında düz metin olarak değil, BCrypt algoritmasıyla tuzlanarak (salt) saklanır.
Yetkilendirme: API uç noktaları [Authorize] öznitelikleri ve JWT token doğrulaması ile korunmaktadır.
Frontend Koruması: Admin ve korumalı sayfalara doğrudan erişim authGuard ve adminGuard ile engellenir.