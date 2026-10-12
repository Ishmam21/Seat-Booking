namespace BLL.Models;

// ADDED: what login returns to the controller, so it never touches the EF User entity or the password hash
public record AuthUser(int Id, string Email, string DisplayName, string Role);