using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using UserDocumentAPI.Services;

namespace UserDocumentAPI.HealthChecks;

public class StorageHealthCheck : IHealthCheck
{
    private readonly FileStorageOptions _options;
    private readonly ILogger<StorageHealthCheck> _logger;

    public StorageHealthCheck(
        IOptions<FileStorageOptions> options,
        ILogger<StorageHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی وجود فولدرها
            if (!Directory.Exists(_options.UploadPath))
                Directory.CreateDirectory(_options.UploadPath);

            if (!Directory.Exists(_options.PdfPath))
                Directory.CreateDirectory(_options.PdfPath);

            // بررسی دسترسی نوشتن
            var testFile = Path.Combine(_options.UploadPath, $"health_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "health check");
            File.Delete(testFile);

            // بررسی فضای دیسک
            var drive = new DriveInfo(Path.GetPathRoot(_options.UploadPath)!);
            var freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);

            var data = new Dictionary<string, object>
            {
                { "upload_path", _options.UploadPath },
                { "pdf_path", _options.PdfPath },
                { "free_space_gb", Math.Round(freeSpaceGB, 2) },
                { "writable", true }
            };

            if (freeSpaceGB < 1) // کمتر از 1GB
                return Task.FromResult(HealthCheckResult.Degraded("Low disk space", null, data));

            return Task.FromResult(HealthCheckResult.Healthy("Storage is healthy", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage is unhealthy", ex));
        }
    }
}
