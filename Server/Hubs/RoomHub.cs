using Microsoft.AspNetCore.SignalR;
using Server.Services;

namespace Server.Hubs;

public class RoomHub : Hub
{
    private readonly RoomManager _roomManager;
    
    public RoomHub(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public virtual async Task JoinGroup(string roomName)
    {
        
    }
}
