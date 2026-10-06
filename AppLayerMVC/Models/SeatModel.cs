using System.ComponentModel.DataAnnotations.Schema;

namespace AppLayerMVC.Models;
public class Seat
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public int X { get; set; }   // column, starts at 1
    public int Y { get; set; }   // row, starts at 1
    public bool IsBooked { get; set; }
    public float Price { get; set; } 
    [NotMapped]
    public bool IsSelected { get; set; } = false;

    public int LayoutId { get; set; }
}
