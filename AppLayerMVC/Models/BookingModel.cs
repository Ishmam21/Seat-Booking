namespace AppLayerMVC.Models;

public class Booking
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";                  // the customer who booked
    public int SeatId { get; set; }                           // which seat they booked
    public DateTime BookedAt { get; set; } = DateTime.UtcNow; // when
}