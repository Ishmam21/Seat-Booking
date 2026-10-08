using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BLL.Models;

namespace AppLayerMVC.Controllers;

public class HomeController : Controller
{
    public static List<Seat> seats = new List<Seat>
        {
            
            new () { Id = 1, Label = "A2", X=2, Y=1, IsBooked = true, Price = 200 },
            new () { Id = 2, Label = "A3", X=3, Y=1, IsBooked = false, Price = 200 },
        
            new () { Id = 3, Label = "B2", X=2, Y=2, IsBooked = false, Price = 200 },
            new () { Id = 4, Label = "B3", X=3, Y=2, IsBooked = false, Price = 200 },
            new () { Id = 5, Label = "C1", X=1, Y=3, IsBooked = false, Price = 400 },
            new () { Id = 6, Label = "C2", X=2, Y=3, IsBooked = false, Price = 400 },
            new () { Id = 7, Label = "C3", X=3, Y=3, IsBooked = false, Price = 400 },
            new () { Id = 8, Label = "C4", X=4, Y=3, IsBooked = true, Price = 400 },
        };
    public IActionResult Index()
    {
        

        

        return RedirectToAction("Index", "Select");
    }
    [HttpPost]
    public IActionResult SelectSeat(int id)
    {
        var seat = CreatorController.layouts.SelectMany(l => l.Seats).FirstOrDefault(s => s.Id == id);

        if (seat == null) return NotFound();

        if (seat.IsBooked) return BadRequest("Seat is already booked.");

        else
        {
            seat.IsSelected = !seat.IsSelected; // Toggle selection
            return Ok();
        }

    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult ViewLayout(int id)   // "id" must be named exactly this: it's filled from the {id?} part of the URL /Home/ViewLayout/3
{
    var layout = CreatorController.layouts.FirstOrDefault(l => l.Id == id);   // null if no layout has that id
    if (layout == null) return NotFound("The requested layout was not found.");   // bad id in the URL: show a 404 instead of crashing

    return View("Index", layout);   // reuse Views/Home/Index.cshtml. The name is needed because without it MVC would look for ViewLayout.cshtml
}
}

