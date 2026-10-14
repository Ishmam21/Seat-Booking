using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repo;

public class UserRepo
{
    private readonly AppDbContext Db;
    public UserRepo(AppDbContext db) => Db = db;

    public Task<User?> GetByEmailAsync(string email) =>
        Db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByIdAsync(int id) =>
        Db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<bool> EmailExistsAsync(string email) =>
        Db.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user)
    {
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        Db.Users.Update(user);
        await Db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await Db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;

        var layoutIds = Db.Layouts.Where(layout => layout.OwnerId == id).Select(layout => layout.Id);
        var seatIds = Db.Seats.Where(seat => layoutIds.Contains(seat.LayoutId)).Select(seat => seat.Id);

        Db.Bookings.RemoveRange(Db.Bookings.Where(booking =>
            booking.UserId == id || seatIds.Contains(booking.SeatId)));
        Db.Seats.RemoveRange(Db.Seats.Where(seat => layoutIds.Contains(seat.LayoutId)));
        Db.Layouts.RemoveRange(Db.Layouts.Where(layout => layout.OwnerId == id));
        Db.Users.Remove(user);

        await Db.SaveChangesAsync();
        return true;
    }
}
