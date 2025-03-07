using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    // TODO
}
