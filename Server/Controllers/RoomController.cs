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
        var room = await _roomManager.CreateRoom(
            GetCurrentUserId(), 
            TellHubGroupPlacingTick, 
            TellHubGroupPlacingTimeout, 
            TellHubGroupLapTick, 
            TellHubGroupLapTimeout,
            TellHubGroupForfeit
            );
        await AlertRoomListHubUpdate();
        return Ok(new { guid = room.Guid });
    }
    
    [HttpGet]
    [Route("{guid}/as-opponent")]
    public ActionResult GetRoomAsOpponentByGuid(string guid)
    {
        var userId = GetCurrentUserId();
        if (!_roomManager.CanSeeRoomAsOpponent(guid, userId))
        {
            return Unauthorized();
        }
        var room = _roomManager.GetRoom(guid)!;
        return Ok(room.ToOpponentProfilData(userId));
    }
    
    [HttpGet]
    [Route("{guid}/as-spectator")]
    public ActionResult GetRoomAsSpectatorByGuid(string guid)
    {
        var userId = GetCurrentUserId();
        if (!_roomManager.CanSeeRoomAsSpectator(guid, userId))
        {
            return Unauthorized();
        }
        var room = _roomManager.GetRoom(guid)!;
        return Ok(room.ToSpectatorProfilData());
    }
    
    [HttpPost]
    [Route("{guid}/place")]
    public async Task<ActionResult> PlaceInRoom(string guid, [FromBody]PlaceInRoomDto dto)
    {
        var playerId = GetCurrentUserId();
        if (!_roomManager.CanPlayerPlaceInRoom(playerId, guid, dto.ShipsOffsets))
        {
            return Unauthorized();
        }
        var room = _roomManager.GetRoom(guid)!;
        _roomManager.PlayerPlaceInRoom(room, playerId, dto.ShipsOffsets);
        await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("PlayerReady", playerId);
        await _roomGoingOnHub.Clients.Group($"{room.Guid}-spectator").SendAsync("PlayerReady", playerId);
        if (room.BothPlayerReady)
        {
            await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("GameOn");
            await _roomGoingOnHub.Clients.Group($"{room.Guid}-spectator").SendAsync("GameOn", room.ToSpectatorProfilData());
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
            Unauthorized();
        }
        var room = _roomManager.GetRoom(guid)!;
        var hit = await _roomManager.PlayerFireInRoom(playerId, room, dto.XOffset, dto.YOffset);
        await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("Move", playerId, dto.XOffset, dto.YOffset, hit);
        await _roomGoingOnHub.Clients.Group($"{room.Guid}-spectator").SendAsync("Move", playerId, dto.XOffset, dto.YOffset, hit);
        if (room.WinnerId is not null)
        {
            await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("PlayerWon", room.WinnerId);
            await _roomGoingOnHub.Clients.Group($"{room.Guid}-spectator").SendAsync("PlayerWon", room.WinnerId);
            await AlertRoomListHubUpdate();
        }
        return Ok(new { hit });
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
        await _roomManager.RemoveUserFromRoom(userId, room);
        if (room.WinnerId is not null)
        {
            await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("PlayerWon", room.WinnerId);
            await _roomGoingOnHub.Clients.Group($"{room.Guid}-spectator").SendAsync("PlayerWon", room.WinnerId);
        }
        await AlertRoomListHubUpdate();
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
            return Unauthorized();
        }
        await _roomManager.JoinRoomAsOpponent(playerId, room);
        await _roomGoingOnHub.Clients.Group(room.Guid).SendAsync("OpponentJoined", room.ToOpponentProfilData(playerId), playerId);
        await AlertRoomListHubUpdate();
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
    
    private async Task TellHubGroupPlacingTick(string roomGuid, int placingSeconds)
    {
        await _roomGoingOnHub.Clients.Group(roomGuid).SendAsync("PlacingTimerTick", placingSeconds);
        await _roomGoingOnHub.Clients.Group($"{roomGuid}-spectator").SendAsync("PlacingTimerTick", placingSeconds);
    }

    private async Task TellHubGroupPlacingTimeout(string roomGuid)
    {
        await _roomGoingOnHub.Clients.Group(roomGuid).SendAsync("PlacingTimerTimeout");
        await _roomGoingOnHub.Clients.Group($"{roomGuid}-spectator").SendAsync("PlacingTimerTimeout");
    }

    private async Task TellHubGroupLapTick(string roomGuid, int lapSeconds)
    {
        await _roomGoingOnHub.Clients.Group(roomGuid).SendAsync("LapTimerTick", lapSeconds);
        await _roomGoingOnHub.Clients.Group($"{roomGuid}-spectator").SendAsync("LapTimerTick", lapSeconds);
    }

    private async Task TellHubGroupLapTimeout(string roomGuid, int lapCount)
    {
        await _roomGoingOnHub.Clients.Group(roomGuid).SendAsync("LapTimerTimeout", lapCount);
        await _roomGoingOnHub.Clients.Group($"{roomGuid}-spectator").SendAsync("LapTimerTimeout", lapCount);
    }

    private async Task TellHubGroupForfeit(string roomGuid, long winnerId)
    {
        await _roomGoingOnHub.Clients.Group(roomGuid).SendAsync("PlayerWon", winnerId);
        await _roomGoingOnHub.Clients.Group($"{roomGuid}-spectator").SendAsync("PlayerWon", winnerId);
    }
    
    public record PlaceInRoomDto
    {
        public required ShipOffsetsDto[] ShipsOffsets { get; set; }
    }

    public record FireInRoomDto
    {
        public required int XOffset { get; set; }
        public required int YOffset { get; set; }
    }
}
