using UserDocumentAPI.Data;
using UserDocumentAPI.Services;
using UserDocumentAPI.Models;
using Microsoft.EntityFrameworkCore;
using Hangfire;

namespace UserDocumentAPI.BackgroundJobs;

public class DocumentProcessingJob
{
    private readonly ILogger<DocumentProcessingJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DocumentProcessingJob(ILogger<DocumentProcessingJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessDocumentAsync(Guid documentId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

        var document = await context.Documents
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
        {
            _logger.LogWarning("Document not found: {DocumentId}", documentId);
            return;
        }

        try
        {
            _logger.LogInformation("🔄 Processing document {DocumentId} for user {UserName}", documentId, document.User.Name);

            // تغییر وضعیت به Processing
            document.Status = DocumentStatus.Processing;
            await context.SaveChangesAsync();

            // تبدیل به PDF
            var pdfFileName = Path.GetFileNameWithoutExtension(document.StoredFileName) + ".pdf";
            var pdfPath = Path.Combine("uploads/pdfs", pdfFileName);

            var success = await fileService.ConvertToPdfAsync(document.FilePath, pdfPath);

            if (success)
            {
                document.Status = DocumentStatus.Completed;
                document.PdfPath = pdfPath;
                document.ProcessedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                _logger.LogInformation("✅ Document processed successfully: {DocumentId}", documentId);

                // ارسال پیام تکمیل
                BackgroundJob.Enqueue<CompletionMessageJob>(job => 
                    job.SendCompletionMessageAsync(document.UserId));
            }
            else
            {
                throw new Exception("PDF conversion failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Document processing failed: {DocumentId}", documentId);
            
            document.Status = DocumentStatus.Failed;
            await context.SaveChangesAsync();
            
            throw; // برای فعال‌سازی Retry
        }
    }
}
