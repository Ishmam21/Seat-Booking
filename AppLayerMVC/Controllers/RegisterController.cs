using Microsoft.AspNetCore.Mvc;
using BLL.Models;

namespace AppLayerMVC.Register;
public class Register : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}