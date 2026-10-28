using System.Security.Claims;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;   // CHANGED: was "using DAL.Storage;"; delete the DAL using



namespace AppLayerMVC.Controllers;

[Authorize(Roles = "Vendor")]
public class MyLayoutsController : Controller
{
    private const long MaxImageSize = 50 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly LayoutService _layouts;
    private readonly IWebHostEnvironment _environment;

    private readonly ImageService _images; 

    public MyLayoutsController(LayoutService layouts, IWebHostEnvironment environment , ImageService images)
    {
        _layouts = layouts;
        _environment = environment;
        _images = images;
    }

    public async Task<IActionResult> Index()
    {
        var ownerId = GetUserId();
        return View(await _layouts.GetByOwnerIdAsync(ownerId));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var layout = await _layouts.GetByIdAsync(id);
        if (layout == null || layout.OwnerId != GetUserId()) return NotFound();
        return View(layout);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSeat(int layoutId, int seatId, string label, float price)
    {
        if (string.IsNullOrWhiteSpace(label) || label.Length > 50 || price <= 0 || float.IsNaN(price) || float.IsInfinity(price))
            return BadRequest("Enter a seat label and a positive price.");

        var updated = await _layouts.UpdateSeatAsync(layoutId, GetUserId(), seatId, label.Trim(), price);
        return updated ? Ok() : NotFound();
    }

        [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadSeatImage(int layoutId, int seatId, IFormFile? seatImage)
    {
        if (seatImage == null || seatImage.Length == 0)
            return RedirectToAction(nameof(Edit), new { id = layoutId });

        var extension = Path.GetExtension(seatImage.FileName);
        if (seatImage.Length > MaxImageSize || !AllowedImageExtensions.Contains(extension))
        {
            TempData["MyLayoutsError"] = "Choose a JPG, PNG, or WEBP image up to 50 MB.";
            return RedirectToAction(nameof(Edit), new { id = layoutId });
        }

        var ownerId = GetUserId();
        var layout = await _layouts.GetByIdAsync(layoutId);
        if (layout == null || layout.OwnerId != ownerId) return NotFound();
        var seat = layout.Seats.FirstOrDefault(item => item.Id == seatId);
        if (seat == null) return NotFound();

        // REMOVED: var directory = Path.Combine(_environment.WebRootPath, ...)  (web server disk path)
        // REMOVED: Directory.CreateDirectory(directory)                          (no local folder needed)
        // REMOVED: var fileName / var filePath                                   (the blob service builds the name)
        // REMOVED: System.IO.File.Create + CopyToAsync                           (that was the write to wwwroot)

        // ADDED: opens the uploaded file as a stream; "await using" closes it when the method ends
        await using var stream = seatImage.OpenReadStream();
        // ADDED: uploads to Blob Storage; returns the full https URL (replaces the "/images/seat-images/..." string)
        var imageUrl = await _images.UploadAsync(stream, seatImage.ContentType, "seat-images", extension);

        if (!await _layouts.UpdateSeatImageAsync(layoutId, ownerId, seatId, imageUrl))
        {
            await _images.DeleteAsync(imageUrl);   // CHANGED: was System.IO.File.Delete(filePath); removes the blob we just uploaded
            return NotFound();
        }

        await _images.DeleteAsync(seat.Img);       // CHANGED: was DeleteImage(seat.Img); removes the old image's blob
        TempData["MyLayoutsMessage"] = "The seat image has been uploaded.";
        return RedirectToAction(nameof(Edit), new { id = layoutId });
    }
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private void DeleteImage(string? imageUrl)
    {
        const string prefix = "/images/seat-images/";
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith(prefix, StringComparison.Ordinal)) return;

        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(_environment.WebRootPath, "images", "seat-images", fileName);
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
    }
}
