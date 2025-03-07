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
        return Ok(user);
    }
    
    [HttpPut]
    [Route("pseudo")]
    public async Task<ActionResult> UpdatePseudo(UpdatePseudoDtoIn dtoIn)
    {
        await _userService.UpdateUserPseudo(User.Identity!.Name!, dtoIn.Pseudo);
        return Ok();
    }
    
    public record UpdatePseudoDtoIn
    {
        [MaxLength(20)]
        [MinLength(3)]
        public required string Pseudo { get; set; }
    }
}