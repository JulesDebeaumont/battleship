using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Server.Services;

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
    
    public int Experience { get; set; }

    public void RegisterGameState(RoomAvailable room)
    {
        var coefficient = 1;
        if (Id == room.WinnerId)
        {
            coefficient += 1;
            WinCount++;
        }
        else
        {
            LooseCount++;
        }
        coefficient += room.LapCount;
        Experience += coefficient * 10;
        GameCount++;
        LapPlayed += (int)Math.Round((double)room.LapCount / 2);
        var opponentSetup = Id == room.PlayerOneId ? room.PlayerTwoSetup : room.PlayerOneSetup;
        ShipDestroyed += opponentSetup!.Ships.Count;
    }
}
