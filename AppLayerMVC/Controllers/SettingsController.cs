using System.Security.Claims;
using BLL.Models;
using BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private const long MaxPictureSize = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedPictureExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private readonly UserService _users;
    private readonly IWebHostEnvironment _environment;

    public SettingsController(UserService users, IWebHostEnvironment environment)
    {
        _users = users;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var settings = await _users.GetAccountSettingsAsync(GetUserId());
        return settings == null ? NotFound() : View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AccountSettingsModel model, IFormFile? profilePicture)
    {
        var userId = GetUserId();
        var current = await _users.GetAccountSettingsAsync(userId);
        if (current == null) return NotFound();

        model.Email = current.Email;
        model.Role = current.Role;
        model.ProfilePictureUrl = current.ProfilePictureUrl;
        ModelState.Remove(nameof(model.Email));

        string? pictureExtension = null;
        if (profilePicture is { Length: > 0 })
        {
            var extension = Path.GetExtension(profilePicture.FileName);
            if (profilePicture.Length > MaxPictureSize)
                ModelState.AddModelError(string.Empty, "Profile pictures must be 5 MB or smaller.");
            else if (!AllowedPictureExtensions.Contains(extension))
                ModelState.AddModelError(string.Empty, "Choose a JPG, PNG, GIF, or WEBP image.");
            else
                pictureExtension = extension;
        }

        if (!ModelState.IsValid) return View(model);

        var newPicturePath = pictureExtension == null
            ? null
            : await SavePictureAsync(profilePicture!, pictureExtension);
        model.ProfilePictureUrl = newPicturePath ?? (model.RemoveProfilePicture ? null : current.ProfilePictureUrl);
        var (user, error) = await _users.UpdateAccountAsync(userId, model);
        if (error != null || user == null)
        {
            if (newPicturePath != null) DeletePicture(newPicturePath);
            model.ProfilePictureUrl = current.ProfilePictureUrl;
            ModelState.AddModelError(nameof(model.CurrentPassword), error ?? "Could not update the account.");
            return View(model);
        }

        await RefreshSignInAsync(user);
        if ((newPicturePath != null || model.RemoveProfilePicture) && !string.IsNullOrWhiteSpace(current.ProfilePictureUrl))
            DeletePicture(current.ProfilePictureUrl);
        TempData["SettingsMessage"] = "Your settings have been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetUserId();
        var current = await _users.GetAccountSettingsAsync(userId);
        if (current == null) return NotFound();

        if (!await _users.DeleteAccountAsync(userId)) return NotFound();
        if (!string.IsNullOrWhiteSpace(current.ProfilePictureUrl)) DeletePicture(current.ProfilePictureUrl);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Select");
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<string> SavePictureAsync(IFormFile picture, string extension)
    {
        var directory = Path.Combine(_environment.WebRootPath, "images", "profile-pictures");
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(directory, fileName);
        await using var file = System.IO.File.Create(filePath);
        await picture.CopyToAsync(file);

        return $"/images/profile-pictures/{fileName}";
    }

    private void DeletePicture(string pictureUrl)
    {
        if (!pictureUrl.StartsWith("/images/profile-pictures/", StringComparison.Ordinal)) return;

        var fileName = Path.GetFileName(pictureUrl);
        var filePath = Path.Combine(_environment.WebRootPath, "images", "profile-pictures", fileName);
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
    }

    private async Task RefreshSignInAsync(AuthUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName),
            new(ClaimTypes.Role, user.Role)
        };

        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
            claims.Add(new Claim("picture", user.ProfilePictureUrl));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
