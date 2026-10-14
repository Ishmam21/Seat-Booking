using System.ComponentModel.DataAnnotations;

namespace BLL.Models;

public class AccountSettingsModel
{
    public string Email { get; set; } = "";

    [Required, StringLength(100)]
    public string DisplayName { get; set; } = "";

    public string Role { get; set; } = "Customer";

    public string? ProfilePictureUrl { get; set; }

    public bool RemoveProfilePicture { get; set; }

    public string? CurrentPassword { get; set; }

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match")]
    public string? ConfirmNewPassword { get; set; }
}
