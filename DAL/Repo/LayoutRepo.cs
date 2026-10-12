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

    public Task<bool> SeatExistsAsync(int seatId) =>
        _db.Seats.AnyAsync(seat => seat.Id == seatId);

    public async Task AddAsync(Layout layout)
    {
        _db.Layouts.Add(layout);
        await _db.SaveChangesAsync();
    }
}
