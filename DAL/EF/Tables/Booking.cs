using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Booking
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int SeatId { get; set; }

    public Guid? BookingGroupId { get; set; }

    public string? GuestName { get; set; }

    public string? GuestEmail { get; set; }

    public string PaymentStatus { get; set; } = "Paid (Demo)";

    public DateTime BookedAt { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual User? User { get; set; }

}
