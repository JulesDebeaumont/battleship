using Microsoft.AspNetCore.SignalR;
using Server.DAL.Repositories;
using Server.Hubs;
using Server.Models;

namespace Server.Services;

public class RoomManager
{
    private const int MaxRoomCount = 10;
    private Dictionary<long, Room> _avaibleRooms = [];
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<RoomHub> _hubContext;

    public RoomManager(IServiceScopeFactory serviceScopeFactory, IHubContext<RoomHub> hubContext)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
    }

    public Room GetRoom(int id)
    {
        return _avaibleRooms[id];
    }

    public async Task CreateRoom(long playerOneId)
    {
        if (_avaibleRooms.Count > MaxRoomCount)
        {
            return;
        }
        var room = new Room
        {
            State = Room.ERoomState.Pending,
            PlayerOneId = playerOneId,
        };
        var roomRepo = GetRoomRepository();
        await roomRepo.CreateRoom(room);
        _avaibleRooms.Add(room.Id, room);
        await _hubContext.Clients.Group("AllRooms").SendAsync("RoomCreated", room);
    }
    
    public int GetRoomLapTimer(int roomId)
    {
        var room = GetRoom(roomId);
        return room.GetTimerInSeconds();
    }

    public void JoinRoomAsPlayer(long playerTwoId, int roomId)
    {
        var room = GetRoom(roomId);
        room.PlayerOneId = playerTwoId;
        room.State = Room.ERoomState.Playing;
        room.StartedAt = DateTime.UtcNow;
        room.StartTimer(TellHubGroupLapSeconds(roomId));
    }

    private Action<int> TellHubGroupLapSeconds(int roomId)
    {
        return (int lapSeconds) => _hubContext.Clients.Group(roomId.ToString()).SendAsync("Timer", lapSeconds);
    }

    public void JoinRoomAsSpectator(int roomId)
    {
        // TODO
    }

    public async Task PlayerConfirmReadiness(int roomId, int playerId, Room.RoomSetup setup)
    {
        // TODO
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("PlayerReady", playerId);
    }

    public async Task PlayerFireInRoom(long playerId, int roomId, int xOffset, int yOffset)
    {
        var room = GetRoom(roomId);
        room.StopTimer();
        if (!PlayerCanPlay(room, playerId)) return;
        var isPlayerOnePlaying = room.PlayerOneId == playerId;
        var roomSetup = isPlayerOnePlaying ? room.PlayerTwoSetup! : room.PlayerOneSetup!;
        roomSetup.FireOffset(xOffset, yOffset);
        if (roomSetup.Dead)
        {
            room.State = Room.ERoomState.Archived;
            room.EndedAt = DateTime.UtcNow;
            _avaibleRooms.Remove(roomId);
            await _hubContext.Clients.Group("AllRooms").SendAsync("RoomRemoved", roomId);
            var roomRepo = GetRoomRepository();
            roomRepo.UpdateRoom(room);
        }
        else
        {
            room.StartTimer(TellHubGroupLapSeconds(roomId));
        }
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("Move", room);
    }

    private static bool PlayerCanPlay(Room room, long playerId)
    {
        if (room.LapCount % 2 == 0)
        {
            return playerId == room.PlayerTwoId;
        }
        return playerId == room.PlayerOneId;
    }
    

    private RoomRepository GetRoomRepository()
    {
        var scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetService<RoomRepository>()!;
    }
    
}