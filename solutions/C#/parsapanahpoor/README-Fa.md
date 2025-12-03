# سیستم پردازش اسناد کاربران

یک Web API قدرتمند با .NET 8 برای ثبت‌نام کاربران همراه با پردازش اسناد، با استفاده از Hangfire برای پردازش Background Jobs.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![Hangfire](https://img.shields.io/badge/Hangfire-1.8.6-blue?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat-square&logo=microsoftsqlserver)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=flat-square&logo=docker)

## 📋 ویژگی‌ها

- **ثبت‌نام کاربر** همراه با آپلود فایل
- **پاسخ سریع API** - تسک‌های پس‌زمینه هرگز API را بلاک نمی‌کنند
- **پردازش Background Jobs** با Hangfire:
  - ✉️ پیام خوش‌آمدگویی (فوری)
  - 📄 پردازش سند با تأخیر ۳۰ ثانیه (شبیه‌سازی تبدیل به PDF)
  - ✅ اعلان تکمیل بعد از پردازش
  - 🧹 پاکسازی شبانه ساعت ۰۰:۰۰
- **سیاست Retry خودکار**:
  - حداکثر ۲ تلاش مجدد
  - تلاش اول: بعد از ۵ دقیقه
  - تلاش دوم: بعد از ۱۰ دقیقه
- **Health Checks** برای دیتابیس، Hangfire و Storage
- **مستندات Swagger**
- **پشتیبانی از Docker**

## 🏗️ معماری

```
UserDocumentProcessing/
├── src/
│   └── UserDocumentAPI/
│       ├── Controllers/        # اندپوینت‌های API
│       ├── Models/             # مدل‌های دامنه و DTOها
│       ├── Services/           # منطق کسب‌وکار
│       ├── BackgroundJobs/     # جاب‌های Hangfire
│       ├── Data/               # EF Core DbContext
│       └── HealthChecks/       # Health Checkهای سفارشی
├── tests/
│   └── UserDocumentAPI.Tests/  # تست‌های واحد
├── docker-compose.yml
└── Dockerfile
```

## 🚀 شروع کار

### پیش‌نیازها

- .NET 8 SDK
- Docker و Docker Compose (پیشنهادی)
- SQL Server (در صورت اجرا بدون Docker)

### روش ۱: Docker (پیشنهادی)

```bash
# کلون کنید و به پوشه پروژه بروید
cd UserDocumentProcessing

# همه سرویس‌ها را اجرا کنید
docker-compose up -d

# صبر کنید تا سرویس‌ها آماده شوند (~۳۰ ثانیه)
```

**آدرس‌های دسترسی:**
- Swagger UI: http://localhost:5000
- داشبورد Hangfire: http://localhost:5000/hangfire
- Health Check: http://localhost:5000/health

### روش ۲: توسعه محلی

۱. **Connection Strings را آپدیت کنید** در `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UserDocumentDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=UserDocumentDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

۲. **برنامه را اجرا کنید:**
```bash
cd src/UserDocumentAPI
dotnet run
```

## 📡 اندپوینت‌های API

### ثبت‌نام کاربر
```http
POST /api/users/register
Content-Type: multipart/form-data

پارامترها:
- Name: string (الزامی)
- Email: string (الزامی، ایمیل معتبر)
- Document: file (الزامی)
```

**پاسخ (201 Created):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Registered",
  "message": "User registered successfully. Document processing will start in 30 seconds."
}
```

### دریافت وضعیت کاربر
```http
GET /api/users/{userId}/status
```

**پاسخ:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "علی محمدی",
  "email": "ali@example.com",
  "document": {
    "status": "Completed",
    "uploadedAt": "2025-12-03T19:00:00Z",
    "processedAt": "2025-12-03T19:00:35Z",
    "pdfPath": "uploads/pdfs/document.pdf"
  }
}
```

## ⚙️ Background Jobs

| جاب | زمان اجرا | توضیحات |
|-----|-----------|---------|
| `WelcomeMessageJob` | فوری | ارسال ایمیل خوش‌آمدگویی به کاربر جدید |
| `DocumentProcessingJob` | تأخیر ۳۰ ثانیه | تبدیل سند به PDF |
| `CompletionMessageJob` | بعد از پردازش | اطلاع‌رسانی تکمیل به کاربر |
| `NightlyCleanupJob` | هر روز ساعت ۰۰:۰۰ | حذف اسناد قدیمی ناموفق |

## 🔄 سیاست Retry

همه Background Jobs دارای retry خودکار هستند:
- **تعداد تلاش:** ۲
- **تأخیرها:** ۵ دقیقه، سپس ۱۰ دقیقه

## 🧪 اجرای تست‌ها

```bash
dotnet test
```

## 📊 Health Checks

به `/health` مراجعه کنید برای وضعیت سیستم:
```json
{
  "status": "Healthy",
  "checks": [
    { "name": "database", "status": "Healthy" },
    { "name": "hangfire", "status": "Healthy" },
    { "name": "storage", "status": "Healthy" }
  ]
}
```

## 🔧 تنظیمات

### appsettings.json
```json
{
  "FileStorage": {
    "UploadPath": "uploads",
    "PdfPath": "uploads/pdfs"
  },
  "Cleanup": {
    "RetentionDays": 7,
    "DeleteFailedDocumentsOnly": true
  }
}
```

## 🐳 سرویس‌های Docker

| سرویس | پورت | توضیحات |
|-------|------|---------|
| API | 5000 | برنامه اصلی |
| SQL Server | 1433 | دیتابیس |

## 📝 تکنولوژی‌ها

- **.NET 8** - فریمورک Web API
- **Entity Framework Core 8** - ORM
- **Hangfire** - پردازش Background Jobs
- **SQL Server 2022** - دیتابیس
- **Serilog** - لاگینگ ساختارمند
- **Swagger/OpenAPI** - مستندات API
- **xUnit و FluentAssertions** - تست‌نویسی
- **Docker** - کانتینرسازی

## 📄 مجوز

این پروژه تحت مجوز MIT منتشر شده است.

## 👤 نویسنده

**پارسا پناه‌پور**

---

⭐ اگر این پروژه برایتان مفید بود، لطفاً ستاره بدهید!

