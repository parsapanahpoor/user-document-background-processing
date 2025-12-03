using Microsoft.Extensions.Options;

namespace UserDocumentAPI.Services;

public class FileStorageOptions
{
    public string UploadPath { get; set; } = "uploads";
    public string PdfPath { get; set; } = "uploads/pdfs";
}

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly FileStorageOptions _options;

    public FileService(ILogger<FileService> logger, IOptions<FileStorageOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        
        // ایجاد پوشه‌ها
        Directory.CreateDirectory(_options.UploadPath);
        Directory.CreateDirectory(_options.PdfPath);
    }

    public async Task<(string StoredFileName, string FilePath)> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_options.UploadPath, storedFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            _logger.LogInformation("File saved: {FilePath}", filePath);
            return (storedFileName, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", file.FileName);
            throw;
        }
    }

    public async Task<bool> ConvertToPdfAsync(string filePath, string pdfPath, CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ شبیه‌سازی تبدیل به PDF (در واقعیت از کتابخانه‌ای مثل iTextSharp استفاده می‌شود)
            _logger.LogInformation("Converting {FilePath} to PDF...", filePath);
            
            await Task.Delay(2000, cancellationToken); // شبیه‌سازی پردازش
            
            // کپی فایل با پسوند PDF
            var pdfFileName = Path.GetFileNameWithoutExtension(filePath) + ".pdf";
            var fullPdfPath = Path.Combine(_options.PdfPath, pdfFileName);
            
            File.Copy(filePath, fullPdfPath, overwrite: true);
            
            _logger.LogInformation("PDF created: {PdfPath}", fullPdfPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF conversion failed: {FilePath}", filePath);
            return false;
        }
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
            }
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            throw;
        }
    }

    public Task<List<string>> GetOldFilesAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldFiles = Directory.GetFiles(_options.UploadPath)
                .Where(f => File.GetCreationTime(f) < threshold)
                .ToList();

            _logger.LogInformation("Found {Count} old files", oldFiles.Count);
            return Task.FromResult(oldFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get old files");
            return Task.FromResult(new List<string>());
        }
    }
}
