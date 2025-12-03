# User Document Processing API

A robust .NET 8 Web API for user registration with document processing, featuring background job processing using Hangfire.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![Hangfire](https://img.shields.io/badge/Hangfire-1.8.6-blue?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat-square&logo=microsoftsqlserver)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=flat-square&logo=docker)

## 📋 Features

- **User Registration** with document upload
- **Fast API Response** - Background tasks never block the API
- **Background Job Processing** with Hangfire:
  - ✉️ Welcome message (immediate)
  - 📄 Document processing with 30-second delay (PDF conversion simulation)
  - ✅ Completion notification after processing
  - 🧹 Nightly cleanup job at 00:00
- **Automatic Retry Policy**:
  - Maximum 2 retries
  - Retry #1: after 5 minutes
  - Retry #2: after 10 minutes
- **Health Checks** for Database, Hangfire, and Storage
- **Swagger Documentation**
- **Docker Support**

## 🏗️ Architecture

```
UserDocumentProcessing/
├── src/
│   └── UserDocumentAPI/
│       ├── Controllers/        # API endpoints
│       ├── Models/             # Domain models & DTOs
│       ├── Services/           # Business logic
│       ├── BackgroundJobs/     # Hangfire jobs
│       ├── Data/               # EF Core DbContext
│       └── HealthChecks/       # Custom health checks
├── tests/
│   └── UserDocumentAPI.Tests/  # Unit tests
├── docker-compose.yml
└── Dockerfile
```

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Docker & Docker Compose (recommended)
- SQL Server (if running without Docker)

### Option 1: Docker (Recommended)

```bash
# Clone and navigate to the project
cd UserDocumentProcessing

# Start all services
docker-compose up -d

# Wait for services to be ready (~30 seconds)
# Access the API
```

**Available Endpoints:**
- Swagger UI: http://localhost:5000
- Hangfire Dashboard: http://localhost:5000/hangfire
- Health Check: http://localhost:5000/health

### Option 2: Local Development

1. **Update Connection Strings** in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UserDocumentDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=UserDocumentDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

2. **Run the application:**
```bash
cd src/UserDocumentAPI
dotnet run
```

## 📡 API Endpoints

### Register User
```http
POST /api/users/register
Content-Type: multipart/form-data

Parameters:
- Name: string (required)
- Email: string (required, valid email)
- Document: file (required)
```

**Response (201 Created):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Registered",
  "message": "User registered successfully. Document processing will start in 30 seconds."
}
```

### Get User Status
```http
GET /api/users/{userId}/status
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "John Doe",
  "email": "john@example.com",
  "document": {
    "status": "Completed",
    "uploadedAt": "2025-12-03T19:00:00Z",
    "processedAt": "2025-12-03T19:00:35Z",
    "pdfPath": "uploads/pdfs/document.pdf"
  }
}
```

## ⚙️ Background Jobs

| Job | Trigger | Description |
|-----|---------|-------------|
| `WelcomeMessageJob` | Immediate | Sends welcome email to new user |
| `DocumentProcessingJob` | 30s delay | Converts document to PDF |
| `CompletionMessageJob` | After processing | Notifies user of completion |
| `NightlyCleanupJob` | Daily at 00:00 | Removes old failed documents |

## 🔄 Retry Policy

All background jobs have automatic retry:
- **Attempts:** 2
- **Delays:** 5 minutes, then 10 minutes

## 🧪 Running Tests

```bash
dotnet test
```

## 📊 Health Checks

Access `/health` for system status:
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

## 🔧 Configuration

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

## 🐳 Docker Services

| Service | Port | Description |
|---------|------|-------------|
| API | 5000 | Main application |
| SQL Server | 1433 | Database |

## 📝 Tech Stack

- **.NET 8** - Web API framework
- **Entity Framework Core 8** - ORM
- **Hangfire** - Background job processing
- **SQL Server 2022** - Database
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **xUnit & FluentAssertions** - Testing
- **Docker** - Containerization

## 📄 License

This project is licensed under the MIT License.

## 👤 Author

**Parsa Panahpoor**

---

⭐ If you found this project helpful, please give it a star!

