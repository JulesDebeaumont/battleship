using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Server.Models;
using Server.Services;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly RoomManager _roomManager;
    private readonly IHubContext<RoomListHub> _roomListHub;
    private readonly IHubContext<RoomGoingOnHub> _roomGoingOnHub;

    public RoomController(RoomManager roomManager, IHubContext<RoomListHub> roomListHub, IHubContext<RoomGoingOnHub> roomGoingOnHub)
    {
        _roomManager = roomManager;
        _roomListHub = roomListHub;
        _roomGoingOnHub = roomGoingOnHub;
    }
    
    [HttpGet]
    public ActionResult GetAvailableRooms()
    {
        return Ok(_roomManager.GetAvailableRooms());
    }

    [HttpPost]
    [Route("new")]
    public async Task<ActionResult> CreateRoom()
    {
        if (!_roomManager.CanCreateRoom())
        {
            return BadRequest("Too much rooms already");
        }
        var room = await _roomManager.CreateRoom(GetCurrentUserId(), TellHubGroupLapSeconds);
        await AlertRoomListHubUpdate();
        return Ok(new { guid = room.Guid });
    }
    
    [HttpGet]
    [Route("{guid}")]
    public ActionResult GetRoomByGuid(string guid)
    {
        var userId = GetCurrentUserId();
        if (_roomManager.CanSeeRoomAsOpponent(guid, userId))
        {
            var room = _roomManager.GetRoom(guid)!;
            return Ok(room.ToOpponentProfilData(userId));
        }
        if (_roomManager.CanSeeRoomAsSpectator(guid, userId))
        {
            var room = _roomManager.GetRoom(guid)!;
            return Ok(room.ToSpectatorProfilData());
        }
        return BadRequest();
    }

    [HttpPost]
    [Route("{guid}/place")]
    public async Task<ActionResult> PlaceInRoom(string guid, [FromBody]PlaceInRoomDto dto)
    {
        var playerId = GetCurrentUserId();
        if (!_roomManager.CanPlayerPlaceInRoom(playerId, guid))
        {
            return BadRequest();
        }
        var room = _roomManager.GetRoom(guid)!;
        _roomManager.PlayerPlaceInRoom(room, playerId, dto.ShipOffsets);
        await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("PlayerReady", playerId);
        if (room.BothPlayerReady)
        {
            await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("GameOn", playerId);
            await AlertRoomListHubUpdate();
        }
        return Ok();
    }

    [HttpPost]
    [Route("{guid}/fire")]
    public async Task<ActionResult> FireInRoom(string guid, [FromBody]FireInRoomDto dto)
    {
        var playerId = GetCurrentUserId();
        if (!_roomManager.CanPlayerFireInRoom(guid, playerId))
        {
            BadRequest();
        }
        var room = _roomManager.GetRoom(guid)!;
        await _roomManager.PlayerFireInRoom(playerId, room, dto.XOffset, dto.YOffset);
        await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("Move",  playerId, dto.XOffset, dto.YOffset);
        if (room.WinnerId is not null)
        {
            await _roomGoingOnHub.Clients.Groups(room.Guid).SendAsync("PlayerWon", room.WinnerId);
            await AlertRoomListHubUpdate();
        }
        return Ok();
    }
    
    [HttpPost]
    [Route("{guid}/leave")]
    public async Task<ActionResult> LeaveRoom(string guid)
    {
        var room = _roomManager.GetRoom(guid);
        if (room == null)
        {
            return Ok();
        }
        var userId = GetCurrentUserId();
        _roomManager.RemoveUserFromRoom(userId, room);
        if (room.WinnerId is not null)
        {
            await _roomGoingOnHub.Clients.Groups(room.Guid).SendAsync("PlayerWon", room.WinnerId);
            await AlertRoomListHubUpdate();
        }

        return Ok();
    }

    [HttpPost]
    [Route("{guid}/join-as-opponent")]
    public async Task<ActionResult> JoinRoomAsOpponent(string guid)
    {
        var playerId = GetCurrentUserId();
        var room = _roomManager.GetRoom(guid)!;
        if (!_roomManager.CanJoinRoomAsOpponent(guid, playerId))
        {
            return BadRequest();
        }
        await _roomManager.JoinRoomAsOpponent(playerId, room);
        await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("OpponentJoined", room);
        await AlertRoomListHubUpdate();
        return Ok();
    }
    
    [HttpPost]
    [Route("{guid}/join-as-spectator")]
    public async Task<ActionResult> JoinRoomAsSpectator(string guid)
    {
        // TODO
        return Ok();
    }

    private long GetCurrentUserId()
    {
        return long.Parse(User.Identity!.Name!);
    }

    private async Task AlertRoomListHubUpdate()
    {
        await _roomListHub.Clients.All.SendAsync("RoomListUpdated");
    }
    
    private async Task TellHubGroupLapSeconds(string roomGuid, int lapSeconds)
    {
        await _roomGoingOnHub.Clients.Group(roomGuid).SendAsync("Timer", lapSeconds);
    }

    public record PlaceInRoomDto
    {
        public required int[][][] ShipOffsets { get; set; }
    }

    public record FireInRoomDto
    {
        public required int XOffset { get; set; }
        public required int YOffset { get; set; }
    }
}
