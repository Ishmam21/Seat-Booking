namespace AppLayerMVC.Models;

public class Layout
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public List<Seat> Seats {get; set;} = new ();
}