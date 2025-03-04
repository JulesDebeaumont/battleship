using Microsoft.AspNetCore.Identity;
using Server.DAL.Repositories;
using Server.Models;

namespace Server.Services;

public class RoomManager
{
    private const int MaxRoomCount = 20;
    private readonly Dictionary<string, RoomAvailable> _availableRooms = [];
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RoomManager(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public List<RoomAvailable.RestrictedRoomDto> GetAvailableRooms()
    {
        return _availableRooms.Select(room => room.Value.ToListProfilData()).ToList();
    }
    
    public RoomAvailable? GetRoom(string guid)
    {
        return _availableRooms[guid];
    }

    public bool CanCreateRoom()
    {
        return _availableRooms.Count < MaxRoomCount;
    }

    public bool CanJoinRoomAsSpectator(string roomGuid, long playerId)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.PlayerOneId == playerId || room.PlayerTwoId == playerId) return false;
        if (room.State != ERoomState.Playing) return false;
        return true;
    }

    public bool CanSeeRoomAsSpectator(string roomGuid, long playerId)
    {
        return CanJoinRoomAsSpectator(roomGuid, playerId);
    }
    
    public bool CanSeeRoomAsOpponent(string roomGuid, long playerId)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.State == ERoomState.Archived) return false;
        if (room.PlayerTwoId != playerId && room.PlayerOneId != playerId) return false;
        return true;
    }
    public bool CanJoinRoomAsOpponent(string roomGuid, long playerTwoId)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.State != ERoomState.Pending) return false;
        if (room.PlayerTwoId != null) return false;
        if (room.PlayerOneId == playerTwoId) return false;
        return true;
    }

    public async Task<RoomAvailable> CreateRoom(long playerOneId, Func<string, int, Task> callbackTimer)
    {
        var userManager = GetUserManager();
        var playerOne = await userManager.FindByIdAsync(playerOneId.ToString());
        var room = new RoomAvailable
        {
            Guid = Guid.NewGuid().ToString(),
            State = ERoomState.Pending,
            PlayerOneId = playerOneId,
            PlayerOne = playerOne,
            CallbackTimer = callbackTimer
        };
        _availableRooms.Add(room.Guid, room);
        return room;
    }

    public async Task JoinRoomAsOpponent(long playerTwoId, RoomAvailable room)
    {
        var userManager = GetUserManager();
        var playerTwo = await userManager.FindByIdAsync(playerTwoId.ToString());
        room.PlayerTwo = playerTwo;
        room.PlayerTwoId = playerTwoId;
        room.State = ERoomState.Placing;
        room.StartedAt = DateTime.UtcNow;
    }

    public void RemoveUserFromRoom(long userId, RoomAvailable room)
    {
        if (room.State is not (ERoomState.Placing or ERoomState.Playing)) return;
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
        if (room.State != ERoomState.Placing) return false;
        if (room.PlayerOneId != playerId && room.PlayerTwoId != playerId) return false;
        if (room.PlayerOneId == playerId && room.PlayerOneSetup != null) return false;
        if (room.PlayerTwoId == playerId && room.PlayerTwoSetup != null) return false;
        return true;
    }

    public void PlayerPlaceInRoom(RoomAvailable room, long playerId, int[][][] shipOffsets)
    {
        var setup = new RoomAvailable.RoomSetup(shipOffsets);
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
        if (room.State != ERoomState.Playing) return false;
        if (room.LapCount % 2 == 0)
        {
            return playerId == room.PlayerTwoId;
        }
        return playerId == room.PlayerOneId;
    }

    public async Task PlayerFireInRoom(long playerId, RoomAvailable room, int xOffset, int yOffset)
    {
        room.StopTimer();
        var isPlayerOnePlaying = room.PlayerOneId == playerId;
        var roomSetup = isPlayerOnePlaying ? room.PlayerTwoSetup! : room.PlayerOneSetup!;
        var hit = roomSetup.FireOffset(xOffset, yOffset);
        room.Moves.Add(new RoomAvailable.RoomMove
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
        return _serviceScopeFactory.CreateScope().ServiceProvider.GetService<RoomRepository>()!;
    }

    private UserManager<User> GetUserManager()
    {
        return _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<UserManager<User>>();
    }
}

public class RoomAvailable
{
    public required string Guid { get; set; }
    public required long PlayerOneId { get; set; }
    public long? PlayerTwoId { get; set; }
    public long? WinnerId { get; set; }
    public required ERoomState State { get; set; }
    public List<RoomMove> Moves { get; } = [];
    public RoomSetup? PlayerOneSetup { get; set; }
    public RoomSetup? PlayerTwoSetup { get; set; }
    public User? PlayerOne { get; set; }
    public User? PlayerTwo { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int LapCount { get; set; } = 1;
    private bool PlayerOneIsReady => PlayerOneSetup?.Ships.Count > 0;
    private bool PlayerTwoIsReady => PlayerTwoSetup?.Ships.Count > 0;
    public bool BothPlayerReady => PlayerOneIsReady && PlayerTwoIsReady;
    private const int LapCountSecond = 15;
    private System.Timers.Timer Timer { get; } = new(1000 * LapCountSecond);
    public required Func<string, int, Task> CallbackTimer { get; set; }

    public void StartTimer()
    {
        Timer.Enabled = true;
        Timer.AutoReset = false;
        Timer.Elapsed += (sender, args) =>
        {
            RunIntervalTimer();
            CallbackTimer?.Invoke(Guid, LapCount);
        };
    }
    
    private void RunIntervalTimer()
    {
        LapCount++;
    }

    public void StopTimer()
    {
        Timer.Enabled = false;
        Timer.AutoReset = false;
        Timer.Stop();
    }

    public void Win(long playerId)
    {
        WinnerId = playerId;
        EndedAt = DateTime.UtcNow;
        State = ERoomState.Archived;
    }

    public void Archived()
    {
        EndedAt = DateTime.UtcNow;
        State = ERoomState.Archived;
    }
    
    public class RoomSetup
    {
        public List<Ship> Ships { get; } = [];
        public List<Tuple<int, int>> FiredOffsets { get; } = [];
        public bool Dead;

        public RoomSetup(int[][][] shipsOffsets)
        {
            foreach (var shipOffset in shipsOffsets)
            {
                Ships.Add(new Ship(shipOffset));
            }
        }

        public class Ship
        {
            private readonly Tuple<int, int>[] _positions;
            public Tuple<int, int>[] Hits { get; private set; } = [];
            public bool IsDrawned { get; private set; }

            public Ship(int[][] positions)
            {
                _positions = positions.Select(p => new Tuple<int, int>(p[0], p[1])).ToArray();
            }

            public void TryHit(int xOffset, int yOffset)
            {
                foreach (var position in _positions)
                {
                    if (position.Item1 == xOffset && position.Item2 == yOffset)
                    {
                        var hit = new Tuple<int, int>(xOffset, yOffset);
                        Hits = Hits.Append(hit).ToArray();
                    }
                }
                if (_positions.Length == Hits.Length)
                {
                    IsDrawned = true;
                }
            }
        }
        
        public bool FireOffset(int xOffset, int yOffset)
        {
            var hitCount = Ships.Aggregate(0, (acc, ship) => acc + ship.Hits.Length);
            FiredOffsets.Add(new Tuple<int, int>(xOffset, yOffset));
            foreach (var ship in Ships)
            {
                ship.TryHit(xOffset, yOffset);
            }
            if (Ships.FindIndex(s => !s.IsDrawned) == -1)
            {
                Dead = true;
            }
            return hitCount != Ships.Aggregate(0, (acc, ship) => acc + ship.Hits.Length);
        }
    }
    
    public record RoomMove
    {
        public int Id { get; set; }
        public int Lap { get; set; }
        public long PlayerId { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public bool Hit { get; set; }
    }
    
    public RestrictedRoomDto ToListProfilData()
    {
        return new RestrictedRoomDto
        {
            Guid = Guid,
            State = State,
            PlayerOne = PlayerOne != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = PlayerOne.Id,
                    Pseudo = PlayerOne.Pseudo,
                    IdRes = PlayerOne.IdRes,
                }
                : null,
            PlayerTwo = PlayerTwo != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = PlayerTwo.Id,
                    Pseudo = PlayerTwo.Pseudo,
                    IdRes = PlayerTwo.IdRes,
                }
                : null,
            StartedAt = StartedAt
        };
    }
    
    public RestrictedRoomDto ToSpectatorProfilData() // TODO
    {
        return new RestrictedRoomDto
        {
            Guid = Guid,
            State = State,
            PlayerOne = PlayerOne != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = PlayerOne.Id,
                    Pseudo = PlayerOne.Pseudo,
                    IdRes = PlayerOne.IdRes,
                }
                : null,
            PlayerTwo = PlayerTwo != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = PlayerTwo.Id,
                    Pseudo = PlayerTwo.Pseudo,
                    IdRes = PlayerTwo.IdRes,
                }
                : null,
            StartedAt = StartedAt
        };
    }
    
    public RestrictedRoomDto ToOpponentProfilData(long playerId) // TODO
    {
        return new RestrictedRoomDto
        {
            Guid = Guid,
            State = State,
            PlayerOne = PlayerOne != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = PlayerOne.Id,
                    Pseudo = PlayerOne.Pseudo,
                    IdRes = PlayerOne.IdRes,
                }
                : null,
            PlayerTwo = PlayerTwo != null
                ? new RestrictedRoomDto.RoomPlayerDto
                {
                    Id = PlayerTwo.Id,
                    Pseudo = PlayerTwo.Pseudo,
                    IdRes = PlayerTwo.IdRes,
                }
                : null,
            StartedAt = StartedAt
        };
    }
    
    public record RestrictedRoomDto
    {
        public required string Guid { get; init; }
        public ERoomState State { get; init; }
        public RoomPlayerDto? PlayerOne { get; init; }
        public RoomPlayerDto? PlayerTwo { get; init; }
        public DateTime? StartedAt { get; init; }

        public record RoomPlayerDto
        {
            public long Id { get; init; }
            public string Pseudo { get; init; } = string.Empty;
            public required string IdRes { get; init; }
        }
    }
}
