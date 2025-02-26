namespace Server.Models;

public class Move
{
    public long Id { get; set; }
    
    public int Lap { get; set; }
    
    public long PlayerId { get; set; }
    
    public int XOffset { get; set; }
    
    public int YOffset { get; set; }
    
    public bool Hit { get; set; }
}
