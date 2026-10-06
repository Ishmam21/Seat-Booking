using Microsoft.AspNetCore.Mvc;
using AppLayerMVC.Models;

namespace AppLayerMVC.Controllers;

public class CreatorController : Controller
{

    public static List<Layout> layouts = new();
    public static List<Seat> seats = new();

    public IActionResult Index()
    {
        return View();
    }

    public static int nextLayoutId = 1; // Static variable to keep track of the next layout ID
    public static int nextSeatId = 1;   // Static variable to keep track of the next seat ID
    [HttpPost]
    public IActionResult SaveLayout([FromBody] Layout newLayoutSeats)
    {
       if (newLayoutSeats == null || newLayoutSeats.Seats.Count == 0)
       {
           return BadRequest("No seats provided.");
       }

       newLayoutSeats.Id = nextLayoutId++; // Assign a unique ID to the new layout

    

       foreach (var seat in newLayoutSeats.Seats)
       {
           seat.Id = nextSeatId++;
           seat.LayoutId = newLayoutSeats.Id; // Associate the seat with the new layout
        
       }
        layouts.Add(newLayoutSeats);
       return Ok();
    }
    
}