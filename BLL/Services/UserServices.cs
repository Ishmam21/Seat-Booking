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

        return new AuthUser(user.Id, user.Email, user.DisplayName, user.Role, user.ProfilePictureUrl);
    }

    public async Task<AccountSettingsModel?> GetAccountSettingsAsync(int id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user == null) return null;

        return new AccountSettingsModel
        {
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }

    public async Task<(AuthUser? User, string? Error)> UpdateAccountAsync(int id, AccountSettingsModel model)
    {
        var user = await _users.GetByIdAsync(id);
        if (user == null) return (null, "Account not found.");

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                return (null, "Enter your current password to change your password.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, workFactor: 12);
        }

        user.DisplayName = model.DisplayName.Trim();
        user.ProfilePictureUrl = model.ProfilePictureUrl;
        await _users.UpdateAsync(user);

        return (new AuthUser(user.Id, user.Email, user.DisplayName, user.Role, user.ProfilePictureUrl), null);
    }

    public Task<bool> DeleteAccountAsync(int id) => _users.DeleteAsync(id);
}
