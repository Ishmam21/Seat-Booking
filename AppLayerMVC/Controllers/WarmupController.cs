using DAL.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLayerMVC.Controllers;

// WHY: Both warm-up routes must stay available before the visitor has an authenticated session.
[AllowAnonymous]
public class WarmupController : Controller
{
    private readonly AppDbContext _db;

    public WarmupController(AppDbContext db) => _db = db;

    // WHY: The initial warm-up response must render without touching Neon.
    [HttpGet]
    public IActionResult Index() => View();

    // WHY: A bounded SELECT 1 wakes Neon while keeping failures generic and preventing a 500 response.
    [HttpGet]
    public async Task<IActionResult> Status()
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(4));
            await _db.Database.OpenConnectionAsync(timeout.Token);
            await using var command = _db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 4;
            await command.ExecuteScalarAsync(timeout.Token);
            return Ok();
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        finally
        {
            try
            {
                if (_db.Database.GetDbConnection().State != System.Data.ConnectionState.Closed)
                    await _db.Database.CloseConnectionAsync();
            }
            catch
            {
                // WHY: Cleanup errors must not escape the handler or turn the status route into a 500.
            }
        }
    }
}
