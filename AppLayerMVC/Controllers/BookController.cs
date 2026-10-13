using System.Diagnostics;
using System.Security.Claims;
using BLL.Models;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

public class BookController : Controller
{
    private readonly LayoutService _layouts;
    private readonly BookingService _bookings;

    public BookController(LayoutService layouts, BookingService bookings)
    {
        _layouts = layouts;
        _bookings = bookings;
    }

    public IActionResult Index() => RedirectToAction("Index", "Select");

    [HttpPost, Authorize(Roles = "Customer"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectSeat(int id)
    {
        if (!await _layouts.SeatExistsAsync(id)) return NotFound();
        if (await _bookings.IsSeatBookedAsync(id)) return BadRequest("Seat is already booked.");
        return Ok();
    }

    [HttpPost, Authorize(Roles = "Customer"), ValidateAntiForgeryToken]
    public async Task<IActionResult> BookSeat(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!await _bookings.BookSeatAsync(id, userId))
            return BadRequest("Seat is unavailable.");

        return Ok();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ViewLayout(int id)
    {
        var layout = await _layouts.GetByIdAsync(id);
        if (layout == null) return NotFound("The requested layout was not found.");
        if (layout.Seats.Count == 0) return NotFound("The requested layout has no seats.");
        return View("Index", layout);
    }
}
