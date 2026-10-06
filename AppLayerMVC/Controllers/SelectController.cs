using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

public class SelectController : Controller
{
    public IActionResult Index()
    {
        return View(CreatorController.layouts);
    }
}