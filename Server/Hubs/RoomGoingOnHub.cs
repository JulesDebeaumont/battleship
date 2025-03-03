using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Services;

namespace Server.Hubs;

[Authorize]
public class RoomGoingOnHub : Hub
{
    private const string AllRoomGroupName = "AllRooms";
    private readonly RoomManager _roomManager;

    public RoomGoingOnHub(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public async Task JoinAllRoomsGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AllRoomGroupName);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        
        return base.OnDisconnectedAsync(exception);
    }

    public async Task CreateRoom()
    {
        if (_roomManager.CanCreateRoom())
        {
            var room = _roomManager.CreateRoom(GetCurrentUserId(), TellHubGroupLapSeconds);
            await Clients.Caller.SendAsync("RoomCreated", room.Id);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
            await Clients.Group(AllRoomGroupName).SendAsync("RoomUpdate");
        }
        else
        {
            await SendPrivateMessage(Context.ConnectionId, "Can't create room");
        }
    }

    public async Task JoinRoom(string roomGuid, EJoinGroupMode mode)
    {
        var playerId = GetCurrentUserId();
        switch (mode)
        {
            case EJoinGroupMode.Opponent:
                if (_roomManager.CanJoinRoomAsOpponent(roomGuid, playerId))
                {
                    var room = _roomManager.GetRoom(roomGuid)!;
                    await _roomManager.JoinRoomAsOpponent(playerId, room);
                    await Clients.Caller.SendAsync("JoinedRoomAsOpponent", roomGuid);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomGuid);
                    await Clients.Group(roomGuid).SendAsync("OpponentJoined", room);
                    await Clients.Group(AllRoomGroupName).SendAsync("RoomUpdate");
                }
                else
                {
                    await SendPrivateMessage(Context.ConnectionId, "Can't join room");
                }
                break;
            
            case EJoinGroupMode.Spectator:
                if (_roomManager.CanJoinRoomAsSpecatator(roomGuid, playerId))
                {
                    await Clients.Caller.SendAsync("JoinedRoomAsSpectator", roomGuid);
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"{roomGuid}-spectator");
                }
                else
                {
                    await SendPrivateMessage(Context.ConnectionId, "Can't join room");
                }
                break;
        }
    }

    public async Task LeaveRoom(string roomGuid)
    {
        var room = _roomManager.GetRoom(roomGuid);
        if (room == null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomGuid);
            await Clients.Caller.SendAsync("LeaveRoom", roomGuid);
            return;
        }
        var userId = GetCurrentUserId();
        _roomManager.RemoveUserFromRoom(userId, room);
        if (room.WinnerId is not null)
        {
            await Clients.Groups(room.Id.ToString()).SendAsync("PlayerWon", room.WinnerId);
            await Clients.Group(AllRoomGroupName).SendAsync("RoomUpdate");
            //await Clients.Groups($"{room.Id.ToString()}-spectator").SendAsync("PlayerWon", room.WinnerId);
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomGuid);
        //await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{room.Id.ToString()}-spectator");
        await Clients.Caller.SendAsync("LeaveRoom", roomGuid);
    }

    public async Task FireInRoom(string roomGuid, int xOffset, int yOffset)
    {
        var playerId = GetCurrentUserId();
        if (_roomManager.CanPlayerFireInRoom(roomGuid, playerId))
        {
            var room = _roomManager.GetRoom(roomGuid)!;
            await _roomManager.PlayerFireInRoom(playerId, room, xOffset, yOffset);
            await Clients.Group(roomGuid).SendAsync("Move",  playerId, xOffset, yOffset);
            if (room.WinnerId is not null)
            {
                await Clients.Groups(room.Id.ToString()).SendAsync("PlayerWon", room.WinnerId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomGuid);
                await Clients.Caller.SendAsync("LeaveRoom", roomGuid);
                await Clients.Group(AllRoomGroupName).SendAsync("RoomUpdate");
            }
        }
    }

    public async Task PlaceInRoom(string roomGuid, int[][][] shipOffsets)
    {
        var playerId = GetCurrentUserId();
        if (_roomManager.CanPlayerPlaceInRoom(playerId, roomGuid))
        {
            var room = _roomManager.GetRoom(roomGuid)!;
            _roomManager.PlayerPlaceInRoom(room, playerId, shipOffsets);
            await Clients.Group(roomGuid).SendAsync("PlayerReady", playerId);
            if (room.BothPlayerReady)
            {
                await Clients.Group(roomGuid).SendAsync("GameOn", playerId);
                await Clients.Group(AllRoomGroupName).SendAsync("RoomUpdate");
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
    
    private void TellHubGroupLapSeconds(string roomGuid, int lapSeconds)
    {
        Clients.Group(roomGuid).SendAsync("Timer", lapSeconds);
    }

    public enum EJoinGroupMode
    {
        Opponent,
        Spectator
    }
}
