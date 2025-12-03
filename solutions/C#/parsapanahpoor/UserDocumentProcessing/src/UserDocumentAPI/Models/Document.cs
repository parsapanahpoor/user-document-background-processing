namespace UserDocumentAPI.Models;

public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public string? PdfPath { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public User User { get; set; } = null!;
}

public enum DocumentStatus
{
    Uploaded,
    Processing,
    Completed,
    Failed
}
