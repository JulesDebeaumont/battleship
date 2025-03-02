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

    public async Task<ResponseService> UpdateUserPseudo(string userId, string pseudo)
    {
        var response = new ResponseService();
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            response.AddError("User not found");
            return response;
        }
        
        user.Pseudo = pseudo;
        await _userManager.UpdateAsync(user);

        return response;
    }
}