using BLL.Models;
using DAL.Repo;
using DAL.EF.Tables;

namespace BLL.Services;

public class BookingService
{
    private readonly BookingRepo _bookings;

    public BookingService(BookingRepo bookings) => _bookings = bookings;

    public Task<bool> IsSeatBookedAsync(int seatId) => _bookings.IsSeatBookedAsync(seatId);

    public Task<bool> CreateBookingsAsync(IReadOnlyCollection<Booking> bookings) =>
        _bookings.CreateBookingsAsync(bookings);

    public async Task<List<MyBookingGroupModel>> GetCustomerBookingsAsync(int customerId)
    {
        var bookings = await _bookings.GetCustomerBookingsAsync(customerId);
        return bookings
            .GroupBy(booking => booking.BookingGroupId.HasValue
                ? $"group:{booking.BookingGroupId.Value}"
                : $"booking:{booking.Id}")
            .Select(group =>
            {
                var rows = group.ToList();
                return new MyBookingGroupModel
                {
                    BookingGroupId = rows[0].BookingGroupId,
                    LayoutName = rows[0].Seat.Layout.Name,
                    BookedAt = rows.Min(booking => booking.BookedAt),
                    Seats = rows.Select(booking => new MyBookedSeatModel
                    {
                        Label = booking.Seat.Label,
                        PaymentStatus = booking.PaymentStatus
                    }).OrderBy(seat => seat.Label).ToList()
                };
            })
            .OrderByDescending(group => group.BookedAt)
            .ToList();
    }

}
