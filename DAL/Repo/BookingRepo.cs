using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DAL.Repo;

public class BookingRepo
{
    private readonly AppDbContext _db;

    public BookingRepo(AppDbContext db) => _db = db;

    public Task<bool> IsSeatBookedAsync(int seatId) =>
        _db.Bookings.AnyAsync(booking => booking.SeatId == seatId);

    public Task<List<Booking>> GetCustomerBookingsAsync(int customerId) =>
        _db.Bookings
            .Where(booking => booking.UserId == customerId)
            .Include(booking => booking.Seat)
                .ThenInclude(seat => seat.Layout)
            .AsNoTracking()
            .OrderByDescending(booking => booking.BookedAt)
            .ToListAsync();

    public async Task<bool> CreateBookingsAsync(IReadOnlyCollection<Booking> bookings)
    {
        _db.Bookings.AddRange(bookings);
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            foreach (var entry in _db.ChangeTracker.Entries<Booking>())
                entry.State = EntityState.Detached;
            return false;
        }
    }

}
