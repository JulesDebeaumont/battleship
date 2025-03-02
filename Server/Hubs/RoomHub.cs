using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Services;

namespace Server.Hubs;

[Authorize]
public class RoomHub : Hub
{
    private readonly RoomManager _roomManager;

    public RoomHub(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public async Task CreateRoom()
    {
        if (_roomManager.CanCreateRoom())
        {
            var room = await _roomManager.CreateRoom(GetCurrentUserId(), TellHubGroupLapSeconds);
            await Clients.Caller.SendAsync("RoomCreated", room.Id);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
            await Clients.Group("AllRooms").SendAsync("RoomCreated", room);
        }
        else
        {
            await SendPrivateMessage(Context.ConnectionId, "Can't create room");
        }
    }

    public async Task JoinRoom(int roomId, EJoinGroupMode mode)
    {
        var playerId = GetCurrentUserId();
        switch (mode)
        {
            case EJoinGroupMode.Opponent:
                if (_roomManager.CanJoinRoomAsOpponent(roomId, playerId))
                {
                    var room = _roomManager.GetRoom(roomId)!;
                    _roomManager.JoinRoomAsOpponent(playerId, room);
                    await Clients.Caller.SendAsync("JoinedRoomAsOpponent", roomId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                    await Clients.Group(roomId.ToString()).SendAsync("OpponentJoined", room);
                }
                else
                {
                    await SendPrivateMessage(Context.ConnectionId, "Can't join room");
                }
                break;
            
            case EJoinGroupMode.Spectator:
                if (_roomManager.CanJoinRoomAsSpecatator(roomId, playerId))
                {
                    var room = _roomManager.GetRoom(roomId)!;
                    await Clients.Caller.SendAsync("JoinedRoomAsSpectator", roomId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                    await Clients.Group(roomId.ToString()).SendAsync("SpectatorJoined", room);
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"{roomId.ToString()}-spectator");
                }
                else
                {
                    await SendPrivateMessage(Context.ConnectionId, "Can't join room");
                }
                break;
        }
        
    }

    public async Task LeaveRoom(int roomId)
    {
        var room = _roomManager.GetRoom(roomId);
        if (room == null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Caller.SendAsync("LeaveRoom", roomId);
            return;
        }
        var userId = GetCurrentUserId();
        _roomManager.RemoveUserFromRoom(userId, room);
        if (room.WinnerId is not null)
        {
            await Clients.Groups(room.Id.ToString()).SendAsync("PlayerWon", room.WinnerId);
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        await Clients.Caller.SendAsync("LeaveRoom", roomId);
    }

    public async Task FireInRoom(int roomId, int xOffset, int yOffset)
    {
        var playerId = GetCurrentUserId();
        if (_roomManager.CanPlayerFireInRoom(roomId, playerId))
        {
            var room = _roomManager.GetRoom(roomId)!;
            _roomManager.PlayerFireInRoom(playerId, room, xOffset, yOffset);
            await Clients.Group(roomId.ToString()).SendAsync("Move",  playerId, xOffset, yOffset);
            if (room.WinnerId is not null)
            {
                await Clients.Groups(room.Id.ToString()).SendAsync("PlayerWon", room.WinnerId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Caller.SendAsync("LeaveRoom", roomId);
            }
        }
    }

    public async Task PlaceInRoom(int roomId, int[][][] shipOffsets)
    {
        var playerId = GetCurrentUserId();
        if (_roomManager.CanPlayerPlaceInRoom(playerId, roomId))
        {
            var room = _roomManager.GetRoom(roomId)!;
            _roomManager.PlayerPlaceInRoom(room, playerId, shipOffsets);
            await Clients.Group(roomId.ToString()).SendAsync("PlayerReady", playerId);
            if (room.BothPlayerReady)
            {
                await Clients.Group(roomId.ToString()).SendAsync("GameOn", playerId);
                // TODO release info to spectator room
            }
        }
    }
    
    private Task SendPrivateMessage(string user, string message)
    {
        return Clients.User(user).SendAsync("ReceivePrivateMessage", message);
    }

    private long GetCurrentUserId()
    {
        return 0L; // TODO
    }
    
    private void TellHubGroupLapSeconds(int roomId, int lapSeconds)
    {
        Clients.Group(roomId.ToString()).SendAsync("Timer", lapSeconds);
    }

    public enum EJoinGroupMode
    {
        Opponent,
        Spectator
    }
}
