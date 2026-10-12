using System.ComponentModel.DataAnnotations;

namespace BLL.Models;

public class RegisterModel
{
    [Required, EmailAddress(ErrorMessage = "Invalid email address"), StringLength(256)]
    public string Email { get; set; } = "";

    [StringLength(100)]
    public string DisplayName { get; set; } = "";

    [Required, MinLength(8,ErrorMessage = "Password must be at least 8 characters long")]
    
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
    public string ConfirmPassword { get; set; } = "";

    [Required, RegularExpression("^(Vendor|Customer)$")]
    public string Role { get; set; } = "Customer";
}