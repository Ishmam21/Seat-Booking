namespace BLL.Models;

public class BookingModel
{
    public int Id { get; set; }
    public int UserId { get; set; }                            // the customer who booked
    public int SeatId { get; set; }                           // which seat they booked
    public DateTime BookedAt { get; set; } = DateTime.UtcNow; // when
}
