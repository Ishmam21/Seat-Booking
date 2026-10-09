using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Layout
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int OwnerId { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
