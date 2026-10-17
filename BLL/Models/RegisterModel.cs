using System.ComponentModel.DataAnnotations;

namespace BLL.Models;

public class RegisterModel
{
    [Required, EmailAddress(ErrorMessage = "Invalid email address"), StringLength(256)]
    public string Email { get; set; } = "";

    [StringLength(100)]
    public string DisplayName { get; set; } = "";

    // WHY: New accounts must meet the requested minimum length and mixed-character password rule on the server and client.
    [Required, RegularExpression(@"^(?=.*[0-9])(?=.*[^A-Za-z0-9\s]).{6,}$", ErrorMessage = "Password must be at least 6 characters and include at least 1 number and 1 special character.")]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
    public string ConfirmPassword { get; set; } = "";

    [Required, RegularExpression("^(Vendor|Customer)$")]
    public string Role { get; set; } = "Customer";
}
