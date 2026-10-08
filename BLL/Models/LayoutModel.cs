namespace BLL.Models;

public class Layout
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public List<Seat> Seats {get; set;} = new ();

    public string OwnerId {get; set;} = "";  // The user who created this layout. Empty string means "no owner" (public layout)
}