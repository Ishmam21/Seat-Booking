namespace BLL.Models;

public class MyBookingGroupModel
{
    public Guid? BookingGroupId { get; set; }
    public string LayoutName { get; set; } = "";
    public DateTime BookedAt { get; set; }
    public List<MyBookedSeatModel> Seats { get; set; } = new();
}

public class MyBookedSeatModel
{
    public string Label { get; set; } = "";
    public string PaymentStatus { get; set; } = "";
}

public class BookingModel
{
    public int Id { get; set; }
    public int? UserId { get; set; }                           // null for guest bookings
    public int SeatId { get; set; }                           // which seat they booked
    public Guid? BookingGroupId { get; set; }
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string PaymentStatus { get; set; } = "Paid (Demo)";
    public DateTime BookedAt { get; set; } = DateTime.UtcNow; // when
}
