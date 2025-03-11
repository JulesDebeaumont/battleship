using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Services;

namespace Server.Hubs;

[Authorize]
public class RoomGoingOnHub : Hub
{
    private readonly RoomManager _roomManager;

    public RoomGoingOnHub(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public async Task JoinRoomOpponentChannel(string roomGuid)
    {
        var userId = GetCurrentUserId();
        if (!_roomManager.CanSeeRoomAsOpponent(roomGuid, userId))
        {
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, roomGuid);
    }

    public async Task JoinRoomSpectatorChannel(string roomGuid)
    {
        var userId = GetCurrentUserId();
        if (!_roomManager.CanSeeRoomAsSpectator(roomGuid, userId))
        {
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{roomGuid}-spectator");
    }

    private long GetCurrentUserId()
    {
        return long.Parse(Context.User!.Identity!.Name!);
    }
    
}
