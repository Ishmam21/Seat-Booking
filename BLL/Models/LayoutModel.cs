namespace BLL.Models;

public class LayoutModel
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public List<SeatModel> Seats {get; set;} = new ();

    public string OwnerId {get; set;} = "";  // The user who created this layout. Empty string means "no owner" (public layout)
}