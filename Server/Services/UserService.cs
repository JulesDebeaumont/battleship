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
}