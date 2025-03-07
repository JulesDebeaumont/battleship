using Microsoft.AspNetCore.Identity;
using Server.Models;
using Server.Services.Utils;

namespace Server.Services;

public class UserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task UpdateUserPseudo(string userId, string pseudo)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ServiceError("User not found", ServiceError.EServiceErrorType.NotFound);
        }
        user.Pseudo = pseudo;
        await _userManager.UpdateAsync(user);
    }

    public async Task<UserProfileDto?> GetUserProfile(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }
        var userProfil = new UserProfileDto
        {
            Id = user.Id,
            IdRes = user.IdRes,
            Pseudo = user.Pseudo,
            GameCount = user.GameCount,
            WinCount = user.WinCount,
            LooseCount = user.LooseCount,
            ShipDestroyed = user.ShipDestroyed,
            LapPlayed = user.LapPlayed,
            RankLeaderboard = user.RankLeaderboard,
            Experience = user.Experience
        };
        return userProfil;
    }

    public record UserProfileDto
    {
        public long Id { get; set; }
        public required string IdRes { get; set; }
        public string Pseudo { get; set; } = string.Empty;
        public int GameCount { get; set; }
        public int WinCount { get; set; }
        public int LooseCount { get; set; }
        public int ShipDestroyed { get; set; }
        public int LapPlayed { get; set; }
        public int RankLeaderboard { get; set; }
        public int Experience { get; set; }
    }
}