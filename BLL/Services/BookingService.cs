using DAL.EF.Tables;
using DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BookingService
{
    private readonly BookingRepo _bookings;

    public BookingService(BookingRepo bookings) => _bookings = bookings;

    public Task<bool> IsSeatBookedAsync(int seatId) => _bookings.IsSeatBookedAsync(seatId);

    public async Task<bool> BookSeatAsync(int seatId, int userId)
    {
        if (await _bookings.IsSeatBookedAsync(seatId))
            return false;

        try
        {
            await _bookings.AddAsync(new Booking { SeatId = seatId, UserId = userId });
            return true;
        }
        catch (DbUpdateException)
        {
            // The unique booking constraint is the final guard against concurrent requests.
            return false;
        }
    }
}
