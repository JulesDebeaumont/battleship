using Microsoft.AspNetCore.SignalR;
using Server.DAL.Repositories;
using Server.Hubs;
using Server.Models;

namespace Server.Services;

public class RoomManager
{
    private const int MaxRoomCount = 20;
    private readonly Dictionary<long, Room> _availableRooms = [];
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RoomManager(IServiceScopeFactory serviceScopeFactory, IHubContext<RoomHub> hubContext)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Room? GetRoom(int id)
    {
        return _availableRooms[id];
    }

    public bool CanCreateRoom()
    {
        return _availableRooms.Count < MaxRoomCount;
    }

    public bool CanJoinRoomAsOpponent(int roomId, long playerTwoId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;
        if (room.State != Room.ERoomState.Pending) return false;
        if (room.PlayerTwoId != null) return false;
        if (room.PlayerOneId == playerTwoId) return false;
        return true;
    }

    public bool CanJoinRoomAsSpecatator(int roomId, long userId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;
        if (room.State is  Room.ERoomState.Archived) return false;
        if (room.PlayerOneId == userId || room.PlayerTwoId == userId) return false;
        return true;
    }

    public async Task<Room> CreateRoom(long playerOneId, Action<int, int> callbackTimer)
    {
        var room = new Room
        {
            State = Room.ERoomState.Pending,
            PlayerOneId = playerOneId,
            CallbackTimer = callbackTimer
        };
        var roomRepo = GetRoomRepository();
        await roomRepo.CreateRoom(room);
        _availableRooms.Add(room.Id, room);
        return room;
    }

    public void JoinRoomAsOpponent(long playerTwoId, Room room)
    {
        room.PlayerTwoId = playerTwoId;
        room.State = Room.ERoomState.Placing;
        room.StartedAt = DateTime.UtcNow;
    }

    public void JoinRoomAsSpectator(Room room)
    {
        // TODO
    }

    public void RemoveUserFromRoom(long userId, Room room)
    {
        if (room.State is not (Room.ERoomState.Placing or Room.ERoomState.Playing)) return;
        if (room.PlayerOneId == userId || room.PlayerTwoId == userId)
        {
            room.Win(userId);
        }
    }

    public bool CanPlayerPlaceInRoom(long playerId, int roomId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;
        return true;
    }

    public void PlayerPlaceInRoom(Room room, long playerId, int[][][] shipOffsets)
    {
        // TODO
        if (room.BothPlayerReady)
        {
            room.StartTimer();
        }
    }

    public bool CanPlayerFireInRoom(int roomId, long playerId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;
        if (room.State != Room.ERoomState.Playing) return false;
        if (room.LapCount % 2 == 0)
        {
            return playerId == room.PlayerTwoId;
        }
        return playerId == room.PlayerOneId;
    }

    public void PlayerFireInRoom(long playerId, Room room, int xOffset, int yOffset)
    {
        room.StopTimer();
        var isPlayerOnePlaying = room.PlayerOneId == playerId;
        var roomSetup = isPlayerOnePlaying ? room.PlayerTwoSetup! : room.PlayerOneSetup!;
        var hit = roomSetup.FireOffset(xOffset, yOffset);
        room.Moves.Add(new Room.RoomMove
        {
            Id = room.Moves.Count + 1,
            Lap = room.LapCount,
            PlayerId = playerId,
            XOffset = xOffset,
            YOffset = yOffset,
            Hit = hit
        });
        if (roomSetup.Dead)
        {
            room.Win(playerId);
            _availableRooms.Remove(room.Id);
            var roomRepo = GetRoomRepository();
            roomRepo.UpdateRoom(room);
        }
        else
        {
            room.StartTimer();
        }
    }

    private RoomRepository GetRoomRepository()
    {
        var scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetService<RoomRepository>()!;
    }
    
}