using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BLL.Models;
using BLL.Services;
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

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectSeat(int id)
    {
        if (!await _layouts.SeatExistsAsync(id)) return NotFound();
        if (await _bookings.IsSeatBookedAsync(id)) return BadRequest("Seat is already booked.");
        return Ok();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int layoutId, int[] seatIds)
    {
        if (IsVendorOrUnsupportedUser()) return Forbid();
        var model = await BuildCheckoutModelAsync(layoutId, seatIds);
        if (model == null) return BadRequest("Choose at least one seat from this layout.");
        if (model.Seats.Any(seat => seat.IsBooked))
            model.ErrorMessage = "One or more selected seats are no longer available. Please choose again.";

        return View("Checkout", model);
    }

    [HttpGet]
    public async Task<IActionResult> ResumeCheckout(int layoutId, string seatIds)
    {
        if (IsVendorOrUnsupportedUser()) return Forbid();
        var ids = seatIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => int.TryParse(value, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToArray();
        var model = await BuildCheckoutModelAsync(layoutId, ids);
        if (model == null) return BadRequest("Choose at least one seat from this layout.");
        if (model.Seats.Any(seat => seat.IsBooked))
            model.ErrorMessage = "One or more selected seats are no longer available. Please choose again.";

        return View("Checkout", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceBooking(int layoutId, int[] seatIds, string? guestName, string? guestEmail, string paymentOutcome)
    {
        if (IsVendorOrUnsupportedUser()) return Forbid();
        var model = await BuildCheckoutModelAsync(layoutId, seatIds);
        if (model == null) return BadRequest("Choose at least one seat from this layout.");

        model.GuestName = guestName?.Trim();
        model.GuestEmail = guestEmail?.Trim();
        if (model.Seats.Any(seat => seat.IsBooked))
            model.ErrorMessage = "One or more selected seats are no longer available. Please choose again.";
        else if (!model.IsSignedInCustomer && (string.IsNullOrWhiteSpace(model.GuestName) || model.GuestName.Length > 100))
            model.ErrorMessage = "Enter your name (up to 100 characters) to continue as a guest.";
        else if (!model.IsSignedInCustomer && !new EmailAddressAttribute().IsValid(model.GuestEmail))
            model.ErrorMessage = "Enter a valid email address to continue as a guest.";
        else if (paymentOutcome is not ("success" or "failure"))
            model.ErrorMessage = "Choose a mock payment result to continue.";

        if (!string.IsNullOrEmpty(model.ErrorMessage)) return View("Checkout", model);

        var userId = model.IsSignedInCustomer
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            : (int?)null;
        var paymentSucceeded = paymentOutcome == "success";
        if (!paymentSucceeded)
        {
            model.StatusMessage = "Mock payment failed. No seats were booked. You can try again.";
            return View("Checkout", model);
        }

        var bookingGroupId = Guid.NewGuid();
        var bookings = model.Seats.Select(seat => new DAL.EF.Tables.Booking
        {
            SeatId = seat.Id,
            BookingGroupId = bookingGroupId,
            UserId = userId,
            GuestName = userId.HasValue ? null : model.GuestName,
            GuestEmail = userId.HasValue ? null : model.GuestEmail,
            PaymentStatus = "Paid (Demo)",
            BookedAt = DateTime.UtcNow
        }).ToList();
        if (!await _bookings.CreateBookingsAsync(bookings))
        {
            model.ErrorMessage = "A selected seat was just booked by someone else. Please choose again.";
            return View("Checkout", model);
        }

        model.IsComplete = true;
        model.StatusMessage = "Mock payment succeeded. Your seats are booked.";
        return View("BookingConfirmation", model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    public async Task<IActionResult> ViewLayout(int id)
    {
        var layout = await _layouts.GetByIdAsync(id);
        if (layout == null) return NotFound("The requested layout was not found.");
        if (layout.Seats.Count == 0) return NotFound("The requested layout has no seats.");
        return View("Index", layout);
    }

    public async Task<IActionResult> Preview360(int id)
    {
        var layouts = await _layouts.GetAllAsync();
        var seat = layouts.SelectMany(layout => layout.Seats).FirstOrDefault(item => item.Id == id);
        if (seat == null || !seat.HasPreview) return NotFound();

        return View("Preview360", seat);
    }

    private bool IsVendorOrUnsupportedUser() =>
        User.Identity?.IsAuthenticated == true && !User.IsInRole("Customer");

    private async Task<CheckoutModel?> BuildCheckoutModelAsync(int layoutId, IEnumerable<int> requestedSeatIds)
    {
        var seatIds = requestedSeatIds.Where(id => id > 0).Distinct().ToList();
        if (seatIds.Count == 0) return null;

        var layout = await _layouts.GetByIdAsync(layoutId);
        if (layout == null) return null;
        var seats = layout.Seats.Where(seat => seatIds.Contains(seat.Id)).ToList();
        if (seats.Count != seatIds.Count) return null;

        var isCustomer = User.IsInRole("Customer");
        var checkoutReturnUrl = Url.Action(nameof(ResumeCheckout), "Book", new
        {
            layoutId,
            seatIds = string.Join(",", seatIds)
        });

        return new CheckoutModel
        {
            LayoutId = layoutId,
            SeatIds = seatIds,
            Seats = seats,
            IsSignedInCustomer = isCustomer,
            CustomerName = User.Identity?.Name,
            CustomerEmail = User.FindFirstValue(ClaimTypes.Email),
            SignInUrl = Url.Action("Index", "Login", new { returnUrl = checkoutReturnUrl })
        };
    }

}
