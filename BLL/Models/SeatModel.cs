using System.ComponentModel.DataAnnotations.Schema;

namespace BLL.Models;
public class Seat
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public int X { get; set; }   // column, starts at 1
    public int Y { get; set; }   // row, starts at 1
    [NotMapped]
    public bool IsBooked { get; set; }
    public float Price { get; set; } 
    [NotMapped]
    public bool IsSelected { get; set; } = false;

    public int LayoutId { get; set; }

    public string? Img { get; set; } // optional image for this seat
}
