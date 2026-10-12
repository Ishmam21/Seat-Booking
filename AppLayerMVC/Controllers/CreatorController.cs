using System.Security.Claims;
using BLL.Models;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

[Authorize(Roles = "Vendor")]
public class CreatorController : Controller
{
    private readonly LayoutService _layouts;

    public CreatorController(LayoutService layouts) => _layouts = layouts;

    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLayout([FromBody] LayoutModel newLayout)
    {
        if (newLayout == null || string.IsNullOrWhiteSpace(newLayout.Name) ||
            newLayout.Seats == null || newLayout.Seats.Count == 0)
            return BadRequest("A layout name and at least one seat are required.");

        if (newLayout.Seats.Any(seat => string.IsNullOrWhiteSpace(seat.Label) || seat.Price <= 0))
            return BadRequest("Each seat needs a label and a positive price.");

        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _layouts.CreateAsync(newLayout, ownerId);
        return Ok();
    }
}
