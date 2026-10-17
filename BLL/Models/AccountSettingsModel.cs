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

    // WHY: Password changes follow the same server-side strength rule as registration while blank remains allowed.
    [RegularExpression(@"^(?=.*[0-9])(?=.*[^A-Za-z0-9\s]).{6,}$", ErrorMessage = "Password must be at least 6 characters and include at least 1 number and 1 special character.")]
    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match")]
    public string? ConfirmNewPassword { get; set; }
}
