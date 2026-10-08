using Microsoft.AspNetCore.Mvc;
using BLL.Models;
namespace AppLayerMVC.Controllers;

public class SelectController : Controller
{
    public IActionResult Index()
    {
        return View(CreatorController.layouts);
    }
}