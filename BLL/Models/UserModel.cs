using System.ComponentModel.DataAnnotations;

namespace BLL.Models;

public class UserModel   // named UserModel from the start so it won't clash with the scaffolded User entity in the DAL
{
    public int Id { get; set; }                       // matches users.id
    [Required,EmailAddress]
    public string Email { get; set; } = "";           // matches users.email, used as the login name
    [Required,StringLength(100)]
    public string DisplayName { get; set; } = "";     // matches users.display_name

    [Required,RegularExpression(@"^Vendor$|^Customer$")]
    public string Role { get; set; } = "Customer";    // "Vendor" or "Customer", same two values the database CHECK allows
    // no PasswordHash here on purpose: the hash stays in the DAL entity so it never reaches a view or JSON
}