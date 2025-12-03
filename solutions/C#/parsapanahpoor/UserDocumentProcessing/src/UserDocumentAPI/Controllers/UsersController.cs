using Microsoft.AspNetCore.Mvc;
using UserDocumentAPI.Models;
using UserDocumentAPI.Data;
using UserDocumentAPI.Services;
using UserDocumentAPI.BackgroundJobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace UserDocumentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;

    public UsersController(
        ILogger<UsersController> logger,
        AppDbContext context,
        IFileService fileService)
    {
        _logger = logger;
        _context = context;
        _fileService = fileService;
    }

    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register(
        [FromForm] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // 1 بررسی ایمیل تکراری
            if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
                return Conflict(new { message = "Email already exists" });

            // 2 ذخیره فایل
            var (storedFileName, filePath) = await _fileService.SaveFileAsync(request.Document, cancellationToken);

            // 3 ساخت کاربر
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                RegisteredAt = DateTime.UtcNow
            };

            // 4 ساخت سند
            var document = new Document
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                OriginalFileName = request.Document.FileName,
                StoredFileName = storedFileName,
                FilePath = filePath,
                FileSize = request.Document.Length,
                ContentType = request.Document.ContentType,
                Status = DocumentStatus.Uploaded,
                UploadedAt = DateTime.UtcNow
            };

            user.Document = document;

            // 5 ذخیره در دیتابیس
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(" User registered: {UserId} - {Email}", user.Id, user.Email);

            // 6 Job های پس‌زمینه

            //  فوری: پیام خوش‌آمدگویی
            BackgroundJob.Enqueue<WelcomeMessageJob>(job => 
                job.SendWelcomeMessageAsync(user.Id));

            // 7 با تأخیر 30 ثانیه: پردازش سند
            BackgroundJob.Schedule<DocumentProcessingJob>(
                job => job.ProcessDocumentAsync(document.Id),
                TimeSpan.FromSeconds(30));

            // 7️⃣ پاسخ سریع
            return StatusCode(201, new RegisterUserResponse
            {
                UserId = user.Id,
                Status = "Registered",
                Message = "User registered successfully. Document processing will start in 30 seconds."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " Registration failed");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{userId:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Document)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            Document = user.Document != null ? new
            {
                user.Document.Status,
                user.Document.UploadedAt,
                user.Document.ProcessedAt,
                user.Document.PdfPath
            } : null
        });
    }
}
