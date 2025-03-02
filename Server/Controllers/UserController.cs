using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }
    
    [HttpPut("update-pseudo")]
    public async Task<ActionResult> UpdatePseudo(UpdatePseudoDtoIn dtoIn)
    {
        await _userService.UpdateUserPseudo(User.Identity!.Name!, dtoIn.Pseudo);
        return Ok();
    }
    
    public record UpdatePseudoDtoIn
    {
        public required string Pseudo { get; set; }
    }
}