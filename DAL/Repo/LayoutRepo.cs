using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repo;

public class LayoutRepo
{
    private readonly AppDbContext _db;

    public LayoutRepo(AppDbContext db) => _db = db;

    public Task<List<Layout>> GetAllAsync() =>
        _db.Layouts.Include(layout => layout.Seats)
            .ThenInclude(seat => seat.Booking)
            .AsNoTracking()
            .ToListAsync();

    public Task<Layout?> GetByIdAsync(int id) =>
        _db.Layouts.Include(layout => layout.Seats)
            .ThenInclude(seat => seat.Booking)
            .AsNoTracking()
            .FirstOrDefaultAsync(layout => layout.Id == id);

    public Task<List<Layout>> GetByOwnerIdAsync(int ownerId) =>
        _db.Layouts.Where(layout => layout.OwnerId == ownerId)
            .Include(layout => layout.Seats)
                .ThenInclude(seat => seat.Booking)
            .AsNoTracking()
            .ToListAsync();

    public async Task<bool> UpdateSeatAsync(int layoutId, int ownerId, int seatId, string label, float price)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(seat => seat.Id == seatId &&
            seat.LayoutId == layoutId && seat.Layout.OwnerId == ownerId);
        if (seat == null) return false;

        seat.Label = label;
        seat.Price = price;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateSeatImageAsync(int layoutId, int ownerId, int seatId, string? imageUrl)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(seat => seat.Id == seatId &&
            seat.LayoutId == layoutId && seat.Layout.OwnerId == ownerId);
        if (seat == null) return false;

        seat.Img = imageUrl;
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> SeatExistsAsync(int seatId) =>
        _db.Seats.AnyAsync(seat => seat.Id == seatId);

    public async Task AddAsync(Layout layout)
    {
        _db.Layouts.Add(layout);
        await _db.SaveChangesAsync();
    }
}
