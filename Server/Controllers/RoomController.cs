using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly RoomManager _roomManager;

    public RoomController(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }
    
    [HttpGet]
    public ActionResult GetAvailableRooms()
    {
        return Ok(_roomManager.GetAvailableRooms());
    }
}
