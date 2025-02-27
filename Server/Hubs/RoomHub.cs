using Microsoft.AspNetCore.SignalR;
using Server.Services;

namespace Server.Hubs;

public class RoomHub : Hub
{
    private readonly RoomSingletonService _roomSingletonService;
    
    public RoomHub(RoomSingletonService roomSingletonService)
    {
        _roomSingletonService = roomSingletonService;
    }

    public virtual async Task JoinGroup(string roomName)
    {
        
    }
}
