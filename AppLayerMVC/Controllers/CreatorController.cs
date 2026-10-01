using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

public class CreatorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}