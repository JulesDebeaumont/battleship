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
    
    public int TotalExperience { get; set; }
    
    public int Level { get; set; }
    
    public int ExperienceScopeNextLevel { get; set; }
    
    public int ExperienceRequiredNextLevel { get; set; }

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
        coefficient *= (int)Math.Round(room.LapCount * 0.5);
        TotalExperience += coefficient * 3;
        GameCount++;
        LapPlayed += Id == room.PlayerOneId ? room.PlayerOneLapPlayedCount : room.PlayerTwoLapPlayedCount;
        var opponentSetup = Id == room.PlayerOneId ? room.PlayerTwoSetup : room.PlayerOneSetup;
        ShipDestroyed += opponentSetup!.Ships.Count;
        CalculateExperienceData();
    }

    public void CalculateExperienceData()
    {
        const int xpByLevel = 40;
        const double ratioByLevel = 1.7;
        const int safetyWhileLimit = 1000;
        var level = 1;
        var deltaExperience = TotalExperience;
        var cumulativeRatio = 1.0;
        var safetyLoopCount = 0;
        while (deltaExperience >= xpByLevel * cumulativeRatio)
        {
            level++;
            deltaExperience -= (int)(xpByLevel * cumulativeRatio);
            cumulativeRatio += ratioByLevel;
            safetyLoopCount++;
            if (safetyLoopCount > safetyWhileLimit)
            {
                break;
            }
        }
        Level = level;
        ExperienceScopeNextLevel = deltaExperience;
        ExperienceRequiredNextLevel = (int)(xpByLevel * cumulativeRatio);
    }
    
}
