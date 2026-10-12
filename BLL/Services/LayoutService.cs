using BLL.Models;
using DAL.EF.Tables;
using DAL.Repo;

namespace BLL.Services;

public class LayoutService
{
    private readonly LayoutRepo _layouts;

    public LayoutService(LayoutRepo layouts) => _layouts = layouts;

    public async Task<List<LayoutModel>> GetAllAsync() =>
        (await _layouts.GetAllAsync()).Select(ToModel).ToList();

    public async Task<LayoutModel?> GetByIdAsync(int id)
    {
        var layout = await _layouts.GetByIdAsync(id);
        return layout == null ? null : ToModel(layout);
    }

    public Task<bool> SeatExistsAsync(int seatId) => _layouts.SeatExistsAsync(seatId);

    public Task CreateAsync(LayoutModel model, int ownerId)
    {
        var layout = new Layout
        {
            Name = model.Name.Trim(),
            OwnerId = ownerId,
            Seats = model.Seats.Select(seat => new Seat
            {
                Label = seat.Label,
                X = seat.X,
                Y = seat.Y,
                Price = seat.Price,
                Img = seat.Img
            }).ToList()
        };

        return _layouts.AddAsync(layout);
    }

    private static LayoutModel ToModel(Layout layout) => new()
    {
        Id = layout.Id,
        Name = layout.Name,
        OwnerId = layout.OwnerId,
        Seats = layout.Seats.Select(seat => new SeatModel
        {
            Id = seat.Id,
            Label = seat.Label,
            X = seat.X,
            Y = seat.Y,
            Price = seat.Price,
            Img = seat.Img,
            LayoutId = seat.LayoutId,
            IsBooked = seat.Booking != null
        }).ToList()
    };
}
