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

    public async Task<List<LayoutModel>> GetByOwnerIdAsync(int ownerId) =>
        (await _layouts.GetByOwnerIdAsync(ownerId)).Select(ToModel).ToList();

    public Task<bool> UpdateSeatAsync(int layoutId, int ownerId, int seatId, string label, float price) =>
        _layouts.UpdateSeatAsync(layoutId, ownerId, seatId, label, price);

    public Task<bool> UpdateSeatImageAsync(int layoutId, int ownerId, int seatId, string? imageUrl) =>
        _layouts.UpdateSeatImageAsync(layoutId, ownerId, seatId, imageUrl);

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

    private static LayoutModel ToModel(Layout layout)
    {
        return new LayoutModel
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
                IsBooked = seat.Booking != null,
                // IDs 1-3 provide sample previews because this project reads seats from PostgreSQL, not an in-memory list.
                ThumbUrl = !string.IsNullOrWhiteSpace(seat.Img) || seat.Id <= 3 ? CreateSampleThumb(seat.Label) : null,
                PreviewImageUrl = seat.Img
            }).ToList()
        };
    }

    private static string CreateSampleThumb(string label)
    {
        var safeLabel = System.Security.SecurityElement.Escape(label) ?? "Seat";
        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='240' height='140'><rect width='100%' height='100%' fill='#cbd5e1'/><text x='50%' y='50%' text-anchor='middle' font-size='22' fill='#334155'>View from {safeLabel}</text></svg>";
        return "data:image/svg+xml;charset=utf-8," + Uri.EscapeDataString(svg);
    }
}
