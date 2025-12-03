using UserDocumentAPI.Data;
using UserDocumentAPI.Services;
using UserDocumentAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace UserDocumentAPI.BackgroundJobs;

public class CleanupOptions
{
    public int RetentionDays { get; set; } = 7;
    public bool DeleteFailedDocumentsOnly { get; set; } = true;
}

public class NightlyCleanupJob
{
    private readonly ILogger<NightlyCleanupJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CleanupOptions _options;

    public NightlyCleanupJob(
        ILogger<NightlyCleanupJob> logger, 
        IServiceProvider serviceProvider,
        IOptions<CleanupOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public async Task CleanupAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

        _logger.LogInformation("🧹 Starting nightly cleanup (retention: {Days} days)...", _options.RetentionDays);

        try
        {
            var threshold = DateTime.UtcNow.AddDays(-_options.RetentionDays);

            var query = context.Documents.Where(d => d.UploadedAt < threshold);

            if (_options.DeleteFailedDocumentsOnly)
            {
                query = query.Where(d => d.Status == DocumentStatus.Failed);
            }

            var oldDocuments = await query.ToListAsync();

            var deletedCount = 0;
            foreach (var doc in oldDocuments)
            {
                try
                {
                    await fileService.DeleteFileAsync(doc.FilePath);

                    if (!string.IsNullOrEmpty(doc.PdfPath))
                        await fileService.DeleteFileAsync(doc.PdfPath);

                    context.Documents.Remove(doc);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete document {DocumentId}", doc.Id);
                }
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("✅ Cleanup completed. Removed {Count} documents", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Cleanup failed");
            throw;
        }
    }
}
