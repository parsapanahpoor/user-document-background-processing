using System.ComponentModel.DataAnnotations;

namespace UserDocumentAPI.Models;

public class RegisterUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Document file is required")]
    public IFormFile Document { get; set; } = null!;
}
