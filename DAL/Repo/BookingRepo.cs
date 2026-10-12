using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repo;

public class BookingRepo
{
    private readonly AppDbContext _db;

    public BookingRepo(AppDbContext db) => _db = db;

    public Task<bool> IsSeatBookedAsync(int seatId) =>
        _db.Bookings.AnyAsync(booking => booking.SeatId == seatId);

    public async Task AddAsync(Booking booking)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
    }
}
