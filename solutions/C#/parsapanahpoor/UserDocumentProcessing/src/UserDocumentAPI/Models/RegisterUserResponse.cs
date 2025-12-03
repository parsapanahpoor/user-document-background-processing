namespace UserDocumentAPI.Models;

public class RegisterUserResponse
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
