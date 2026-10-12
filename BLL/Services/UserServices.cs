using BLL.Models;
using DAL.EF.Tables;
using DAL.Repo;

namespace BLL.Services;

public class UserService
{
    private readonly UserRepo _users;
    public UserService(UserRepo users) => _users = users;

    // returns null on success, or an error message
    public async Task<string?> RegisterAsync(RegisterModel m)
    {
        var email = m.Email.Trim().ToLower();
        if (await _users.EmailExistsAsync(email))
            return "That email is already registered.";

        var user = new User
        {
            Email = email,
            DisplayName = m.DisplayName?.Trim() ?? "",
            Role = m.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(m.Password, workFactor: 12)
        };

        await _users.AddAsync(user);
        return null;
    }

    // returns the user on success, or null if email/password is wrong
    public async Task<AuthUser?> LoginAsync(LoginModel m)
    {
        var user = await _users.GetByEmailAsync(m.Email.Trim().ToLower());
        if (user == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(m.Password, user.PasswordHash))
            return null;

        return new AuthUser(user.Id, user.Email, user.DisplayName, user.Role);
    }
}
