using Microsoft.AspNetCore.Identity;
using Server.DAL.Repositories;
using Server.Models;

namespace Server.Services;

public class RoomManager
{
    private const int MaxRoomCount = 20;
    private readonly Dictionary<string, Room> _availableRooms = [];
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly UserManager<User> _userManager;

    public RoomManager(IServiceScopeFactory serviceScopeFactory, UserManager<User> userManager)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userManager = userManager;
    }

    public List<RestrictedRoomDto> GetAvailableRooms()
    {
        return _availableRooms.Select(room => ProjectRoomToRestrictedData(room.Value)).ToList();
    }

    private RestrictedRoomDto ProjectRoomToRestrictedData(Room room)
    {
        return new RestrictedRoomDto
        {
            Guid = room.Guid,
            State = room.State,
            PlayerOne = room.PlayerOne != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = room.PlayerOne.Id,
                    Pseudo = room.PlayerOne.Pseudo,
                    IdRes = room.PlayerOne.IdRes,
                }
                : null,
            PlayerTwo = room.PlayerTwo != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = room.PlayerTwo.Id,
                    Pseudo = room.PlayerTwo.Pseudo,
                    IdRes = room.PlayerTwo.IdRes,
                }
                : null,
            CreatedAt = room.CreatedAt,
            StartedAt = room.StartedAt
        };
    }
    
    public record RestrictedRoomDto
    {
        public required string Guid { get; init; }
        public Room.ERoomState State { get; init; }
        public RoomPlayerDto? PlayerOne { get; init; }
        public RoomPlayerDto? PlayerTwo { get; init; }
        public byte[] CreatedAt { get; init; } = [];
        public DateTime StartedAt { get; init; }

        public record RoomPlayerDto
        {
            public long Id { get; init; }
            public string Pseudo { get; init; } = string.Empty;
            public required string IdRes { get; init; }
        }
    }

    public Room? GetRoom(string id)
    {
        return _availableRooms[id];
    }

    public bool CanCreateRoom()
    {
        return _availableRooms.Count < MaxRoomCount;
    }

    public bool CanJoinRoomAsOpponent(string roomGuid, long playerTwoId)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.State != Room.ERoomState.Pending) return false;
        if (room.PlayerTwoId != null) return false;
        if (room.PlayerOneId == playerTwoId) return false;
        return true;
    }

    public bool CanJoinRoomAsSpecatator(string roomGuid, long userId)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.State is  Room.ERoomState.Archived) return false;
        if (room.PlayerOneId == userId || room.PlayerTwoId == userId) return false;
        return true;
    }

    public async Task<Room> CreateRoom(long playerOneId, Action<string, int> callbackTimer)
    {
        var playerOne = await _userManager.FindByIdAsync(playerOneId.ToString());
        var room = new Room
        {
            Guid = Guid.NewGuid().ToString(),
            State = Room.ERoomState.Pending,
            PlayerOneId = playerOneId,
            PlayerOne = playerOne,
            CallbackTimer = callbackTimer
        };
        _availableRooms.Add(room.Guid, room);
        return room;
    }

    public async Task JoinRoomAsOpponent(long playerTwoId, Room room)
    {
        var playerTwo = await _userManager.FindByIdAsync(playerTwoId.ToString());
        room.PlayerTwo = playerTwo;
        room.PlayerTwoId = playerTwoId;
        room.State = Room.ERoomState.Placing;
        room.StartedAt = DateTime.UtcNow;
    }

    public void RemoveUserFromRoom(long userId, Room room)
    {
        if (room.State is not (Room.ERoomState.Placing or Room.ERoomState.Playing)) return;
        if (room.PlayerOneId == userId)
        {
            if (room.PlayerTwoId != null)
            {
                room.Win(room.PlayerTwoId ?? 0L); // wtf
            }
            else
            {
                room.Archived();
            }
        }
        if (room.PlayerTwoId == userId)
        {
            room.Win(room.PlayerOneId);
        }
    }

    public bool CanPlayerPlaceInRoom(long playerId, string roomGuid)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.State != Room.ERoomState.Placing) return false;
        if (room.PlayerOneId != playerId && room.PlayerTwoId != playerId) return false;
        if (room.PlayerOneId == playerId && room.PlayerOneSetup != null) return false;
        if (room.PlayerTwoId == playerId && room.PlayerTwoSetup != null) return false;
        return true;
    }

    public void PlayerPlaceInRoom(Room room, long playerId, int[][][] shipOffsets)
    {
        var setup = new Room.RoomSetup(shipOffsets);
        if (playerId == room.PlayerOneId)
        {
            room.PlayerOneSetup = setup;
        }
        else
        {
            room.PlayerTwoSetup = setup;
        }
        if (room.BothPlayerReady)
        {
            room.StartTimer();
        }
    }

    public bool CanPlayerFireInRoom(string roomGuid, long playerId)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.State != Room.ERoomState.Playing) return false;
        if (room.LapCount % 2 == 0)
        {
            return playerId == room.PlayerTwoId;
        }
        return playerId == room.PlayerOneId;
    }

    public async Task PlayerFireInRoom(long playerId, Room room, int xOffset, int yOffset)
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
            _availableRooms.Remove(room.Guid);
            var roomRepo = GetRoomRepository();
            await roomRepo.CreateRoom(room);
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