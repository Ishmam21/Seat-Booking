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

    public Task<bool> EmailExistsAsync(string email) =>
        Db.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user)
    {
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
    }
}