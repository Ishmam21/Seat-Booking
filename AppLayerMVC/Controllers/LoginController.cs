using System.Security.Claims;
using BLL.Models;
using BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

public class LoginController : Controller
{
    private readonly UserService _users;
    public LoginController(UserService users) => _users = users;

    [HttpGet]
    public IActionResult Index(string? returnUrl)
    {
        SetModeNotice(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            SetModeNotice(returnUrl);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var user = await _users.LoginAsync(model);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            SetModeNotice(returnUrl);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Select");
    }

    private void SetModeNotice(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return;

        var isVendorPage = returnUrl.StartsWith("/Creator", StringComparison.OrdinalIgnoreCase);
        var isCustomerPage = returnUrl.StartsWith("/Select", StringComparison.OrdinalIgnoreCase) ||
                             returnUrl.StartsWith("/Book", StringComparison.OrdinalIgnoreCase);

        if (isVendorPage)
            ViewData["ModeNotice"] = "This page is for Vendor mode. Log in with a Vendor account to continue.";
        else if (isCustomerPage)
            ViewData["ModeNotice"] = "This page is for Customer mode. Log in with a Customer account to continue.";
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Select");
    }
}
