using BLL.Models;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;   // was AppLayerMVC.Register, change it

public class Register : Controller
{
    private readonly UserService _users;
    public Register(UserService users) => _users = users;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegisterModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var error = await _users.RegisterAsync(model);
        if (error != null)
        {
            ModelState.AddModelError(nameof(model.Email), error);
            return View(model);
        }
        return RedirectToAction("Index", "Login");
    }
}