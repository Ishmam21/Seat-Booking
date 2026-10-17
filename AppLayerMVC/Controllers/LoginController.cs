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
        // WHY: An authenticated visitor should not be shown a second login form.
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Select");

        SetModeNotice(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginModel model, string? returnUrl)
    {
        // WHY: Block direct POSTs to login after authentication, matching the GET behavior.
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Select");

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
        // WHY: Cookie authentication is stateless here; signing out expires the full auth cookie (no server session is configured).
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Select");
    }
}
