using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Seat
{
    public int Id { get; set; }

    public string Label { get; set; } = null!;

    public int X { get; set; }

    public int Y { get; set; }

    public float Price { get; set; }

    public int LayoutId { get; set; }

    public string? Img { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Layout Layout { get; set; } = null!;
}
