namespace BLL.Models;

public class CheckoutModel
{
    public int LayoutId { get; set; }
    public List<int> SeatIds { get; set; } = new();
    public List<SeatModel> Seats { get; set; } = new();
    public bool IsSignedInCustomer { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? SignInUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StatusMessage { get; set; }
    public bool IsComplete { get; set; }
    public decimal Total => Seats.Sum(seat => decimal.Round((decimal)seat.Price, 2));
}
