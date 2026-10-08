using Microsoft.AspNetCore.Mvc;
using BLL.Models;

namespace AppLayerMVC.Controllers;
public class Login : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}