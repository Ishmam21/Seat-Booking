namespace BLL.Models;

public class LayoutModel
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public List<SeatModel> Seats {get; set;} = new ();

    public int OwnerId {get; set;}

}
