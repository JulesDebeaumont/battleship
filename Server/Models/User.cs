using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Server.Models;

public class User : IdentityUser<long>
{
    [Required]
    [MaxLength(7)]
    [MinLength(7)]
    public required string IdRes { get; set; }

    [MaxLength(20)]
    [MinLength(3)]
    public string Pseudo { get; set; } = string.Empty;
    
    public int GameCount { get; set; }
    
    public int WinCount { get; set; }
    
    public int LooseCount { get; set; }
    
    public int ShipDestroyed { get; set; }
    
    public int LapPlayed { get; set; }
    
    public int RankLeaderboard { get; set; }
    
    public int Level { get; set; }
}
