namespace UserDocumentAPI.Services;

public interface IFileService
{
    Task<(string StoredFileName, string FilePath)> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<bool> ConvertToPdfAsync(string filePath, string pdfPath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<List<string>> GetOldFilesAsync(DateTime threshold, CancellationToken cancellationToken = default);
}
