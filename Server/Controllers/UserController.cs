using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userService.GetUserProfile(User.Identity!.Name!);
        if (user == null)
        {
            return Unauthorized();
        }
        return Ok(user);
    }
    
    [HttpPut]
    [Route("pseudo")]
    public async Task<ActionResult> UpdatePseudo(UpdatePseudoDtoIn dtoIn)
    {
        await _userService.UpdateUserPseudo(User.Identity!.Name!, dtoIn.Pseudo);
        return Ok();
    }

    [HttpGet]
    [Route("leaderboard")]
    public async Task<ActionResult> GetLeaderboard()
    {
        var leaderboard = await _userService.GetLeaderboard();
        return Ok(leaderboard);
    }
    
    public record UpdatePseudoDtoIn
    {
        [MaxLength(20)]
        [MinLength(3)]
        public required string Pseudo { get; set; }
    }
}