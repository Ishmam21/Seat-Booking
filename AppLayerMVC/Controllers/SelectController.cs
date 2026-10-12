using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

public class SelectController : Controller
{
    private readonly LayoutService _layouts;

    public SelectController(LayoutService layouts) => _layouts = layouts;

    public async Task<IActionResult> Index() => View(await _layouts.GetAllAsync());
}
