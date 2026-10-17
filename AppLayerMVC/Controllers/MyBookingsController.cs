using System.Security.Claims;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayerMVC.Controllers;

[Authorize(Roles = "Customer")]
public class MyBookingsController : Controller
{
    private readonly BookingService _bookings;

    public MyBookingsController(BookingService bookings) => _bookings = bookings;

    public async Task<IActionResult> Index()
    {
        var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return View(await _bookings.GetCustomerBookingsAsync(customerId));
    }
}
