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

    public List<RoomAvailable.RoomFromListDto> GetAvailableRooms()
    {
        return _availableRooms.Select(room => room.Value.ToListProfilData()).ToList();
    }
    
    public RoomAvailable? GetRoom(string guid)
    {
        return _availableRooms.TryGetValue(guid, out var room) ? room : null;
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

    public async Task<RoomAvailable> CreateRoom(long playerOneId, Func<string, int, Task> callbackTimerTick, Func<string, int, Task> callbackTimeout)
    {
        var userManager = GetUserManager();
        var playerOne = await userManager.FindByIdAsync(playerOneId.ToString());
        var room = new RoomAvailable
        {
            Guid = Guid.NewGuid().ToString(),
            State = ERoomState.Pending,
            PlayerOneId = playerOneId,
            PlayerOne = playerOne,
            CallbackTimerTick = callbackTimerTick,
            CallbackTimerTimeout = callbackTimeout
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

    public async Task RemoveUserFromRoom(long userId, RoomAvailable room)
    {
        if (room.State is not (ERoomState.Placing or ERoomState.Playing or ERoomState.Pending)) return;
        if (room.PlayerOneId == userId)
        {
            if (room.PlayerTwoId != null)
            {
                await RoomWinAndSave(room, room.PlayerTwoId ?? 0L);  // wtf
            }
            else
            {
                room.Archived();
                _availableRooms.Remove(room.Guid);
            }
        }
        if (room.PlayerTwoId == userId)
        {
            await RoomWinAndSave(room, room.PlayerOneId);
        }
    }

    public bool CanPlayerPlaceInRoom(long playerId, string roomGuid, ShipOffsetsDto[] shipOffsets)
    {
        var room = GetRoom(roomGuid);
        if (room == null) return false;
        if (room.State != ERoomState.Placing) return false;
        if (room.PlayerOneId != playerId && room.PlayerTwoId != playerId) return false;
        if (room.PlayerOneId == playerId && room.PlayerOneSetup != null) return false;
        if (room.PlayerTwoId == playerId && room.PlayerTwoSetup != null) return false;
        if (!AreShipsOffsetsOk(shipOffsets)) return false;
        return true;
    }

    private bool AreShipsOffsetsOk(ShipOffsetsDto[] shipOffsets)
    {
        if (shipOffsets.Length != 3) return false;
        var uniqLength = new Dictionary<int, bool>
        {
            { 3, false },
            { 4, false },
            { 5, false }
        };
        var flatListOfOffsets = new Dictionary<Tuple<int, int>, bool>();
        var ok = true;
        foreach (var ship in shipOffsets)
        {
            if (uniqLength.TryGetValue(ship.Offsets.Length, out var shipOffset))
            {
                if (shipOffset)
                {
                    ok = false;
                    break;
                }
                uniqLength[ship.Offsets.Length] = true;
            }
            else
            {
                ok = false;
                break;
            }
            foreach (var offset in ship.Offsets)
            {
                var tryAdd = flatListOfOffsets.TryAdd(new Tuple<int, int>(offset.XOffset, offset.YOffset), true);
                if (!tryAdd || offset.XOffset > 7 || offset.YOffset > 7 || offset.XOffset < 0 || offset.YOffset < 0)
                {
                    ok = false;
                    break;
                }
            }
            if (!ok) break;
        }

        if (!ok) return false;
        if (uniqLength.Count(len => len.Value == false) > 0) return false;
        return true;
    }

    public void PlayerPlaceInRoom(RoomAvailable room, long playerId, ShipOffsetsDto[] shipOffsets)
    {
        var setup = new RoomAvailable.RoomSetup(shipOffsets);
        if (playerId == room.PlayerOneId)
        {
            room.PlayerOneSetup = setup;
        }
        if (playerId == room.PlayerTwoId)
        {
            room.PlayerTwoSetup = setup;
        }
        if (room.BothPlayerReady)
        {
            room.State = ERoomState.Playing;
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

    public async Task<bool> PlayerFireInRoom(long playerId, RoomAvailable room, int xOffset, int yOffset)
    {
        room.StopTimer();
        var isPlayerOnePlaying = room.PlayerOneId == playerId;
        var roomSetup = isPlayerOnePlaying ? room.PlayerTwoSetup! : room.PlayerOneSetup!;
        var hit = roomSetup.FireOffset(xOffset, yOffset);
        if (isPlayerOnePlaying)
        {
            room.PlayerOneLapPlayedCount++;
        }
        else
        {
            room.PlayerTwoLapPlayedCount++;
        }
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
            await RoomWinAndSave(room, playerId);
        }
        else
        {
            room.LapCount++;
            room.StartTimer();
        }
        return hit;
    }

    private async Task RoomWinAndSave(RoomAvailable room, long playerId)
    {
        room.Win(playerId);
        _availableRooms.Remove(room.Guid);
        var roomRepo = GetRoomRepository();
        await roomRepo.CreateRoom(room);
        var userManager = GetUserManager();
        var playerOne = await userManager.FindByIdAsync(room.PlayerOneId.ToString());
        playerOne!.RegisterGameState(room);
        await userManager.UpdateAsync(playerOne);
        var playerTwo = await userManager.FindByIdAsync(room.PlayerTwoId.ToString()!);
        playerTwo!.RegisterGameState(room);
        await userManager.UpdateAsync(playerTwo);
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
    private int TimerProgress { get; set; }
    private System.Timers.Timer Timer { get; } = new(1000);
    public required Func<string, int, Task> CallbackTimerTimeout { get; set; }
    public required Func<string, int, Task> CallbackTimerTick { get; set; }
    public int PlayerOneLapPlayedCount { get; set; }
    public int PlayerTwoLapPlayedCount { get; set; }

    public void StartTimer()
    {
        Timer.Enabled = true;
        Timer.AutoReset = true;
        Timer.Elapsed += RunIntervalTimer;
    }
    
    private void RunIntervalTimer(object? sender, System.Timers.ElapsedEventArgs eventArgs)
    {
        CallbackTimerTick.Invoke(Guid, TimerProgress);
        TimerProgress++;
        if (TimerProgress >= LapCountSecond)
        {
            LapCount++;
            CallbackTimerTimeout.Invoke(Guid, LapCount);
            TimerProgress = 0;
        }
    }

    public void StopTimer()
    {
        Timer.Enabled = false;
        Timer.AutoReset = false;
        Timer.Elapsed -= RunIntervalTimer;
        Timer.Stop();
        TimerProgress = 0;
    }

    public void Win(long playerId)
    {
        WinnerId = playerId;
        EndedAt = DateTime.UtcNow;
        State = ERoomState.Archived;
        StopTimer();
    }

    public void Archived()
    {
        EndedAt = DateTime.UtcNow;
        State = ERoomState.Archived;
        StopTimer();
    }
    
    public class RoomSetup
    {
        public List<Ship> Ships { get; } = [];
        public List<OffsetsWithHitDto> FiredOffsets { get; } = [];
        public bool Dead;

        public RoomSetup(ShipOffsetsDto[] shipsOffsets)
        {
            foreach (var shipOffset in shipsOffsets)
            {
                Ships.Add(new Ship(shipOffset));
            }
        }

        public class Ship
        {
            public readonly OffsetsWithHitDto[] Positions;
            public bool IsDrawned { get; private set; }
            public string Guid { get; set; }
            public string Type { get; set; }
            public string Orientation { get; set; }

            public Ship(ShipOffsetsDto dataOffsets)
            {
                Positions = dataOffsets.Offsets.Select(p => new OffsetsWithHitDto
                {
                    XOffset = p.XOffset,  
                    YOffset = p.YOffset, 
                    Hit = false
                }).ToArray();
                Guid = dataOffsets.Guid;
                Type = dataOffsets.Type;
                Orientation = dataOffsets.Orientation;
            }

            public void TryHit(int xOffset, int yOffset)
            {
                foreach (var position in Positions)
                {
                    if (position.XOffset == xOffset && position.YOffset == yOffset)
                    {
                        position.Hit = true;
                    }
                }
                if (Positions.FirstOrDefault(p => !p.Hit) == null)
                {
                    IsDrawned = true;
                }
            }
        }
        
        public bool FireOffset(int xOffset, int yOffset)
        {
            var hitCount = Ships.Aggregate(0, (acc, ship) => acc + ship.Positions.Count(p => p.Hit));
            foreach (var ship in Ships)
            {
                ship.TryHit(xOffset, yOffset);
            }
            if (Ships.FindIndex(s => !s.IsDrawned) == -1)
            {
                Dead = true;
            }
            var hasHit = hitCount != Ships.Aggregate(0, (acc, ship) => acc + ship.Positions.Count(p => p.Hit));
            FiredOffsets.Add(new OffsetsWithHitDto() { XOffset = xOffset, YOffset = yOffset, Hit = hasHit });
            return hasHit;
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
    
    public RoomFromListDto ToListProfilData()
    {
        return new RoomFromListDto
        {
            Guid = Guid,
            State = State,
            PlayerOne = PlayerOne != null
                ? new RoomFromListDto.RoomPlayerDto
                {
                    Id = PlayerOne.Id,
                    Pseudo = PlayerOne.Pseudo,
                }
                : null,
            PlayerTwo = PlayerTwo != null
                ? new RoomFromListDto.RoomPlayerDto
                {
                    Id = PlayerTwo.Id,
                    Pseudo = PlayerTwo.Pseudo,
                }
                : null,
            StartedAt = StartedAt
        };
    }
    
    public RoomOpponentDto ToOpponentProfilData(long userId)
    {
        return new RoomOpponentDto
        {
            Guid = Guid,
            State = State,
            PlayerOne = PlayerOne != null
                ? new RoomOpponentDto.RoomPlayerDto
                {
                    Id = PlayerOne.Id,
                    Pseudo = PlayerOne.Pseudo,
                }
                : null,
            UserSetup = PlayerOneId == userId ? PlayerOneSetup : PlayerTwoSetup,
            UserFiredOffsets = PlayerOneId == userId ? PlayerOneSetup?.FiredOffsets : PlayerTwoSetup?.FiredOffsets,
            PlayerTwo = PlayerTwo != null
                ? new RoomOpponentDto.RoomPlayerDto
                {
                    Id = PlayerTwo.Id,
                    Pseudo = PlayerTwo.Pseudo,
                }
                : null,
            OpponentFiredOffsets = PlayerOneId == userId ? PlayerTwoSetup?.FiredOffsets : PlayerOneSetup?.FiredOffsets,
            StartedAt = StartedAt
        };
    }
    
    public RoomSpectatorDto ToSpectatorProfilData()
    {
        return new RoomSpectatorDto
        {
            Guid = Guid,
            State = State,
            PlayerOne = PlayerOne != null
                ? new RoomSpectatorDto.RoomPlayerDto
                {
                    Id = PlayerOne.Id,
                    Pseudo = PlayerOne.Pseudo,
                }
                : null,
            PlayerOneSetup = PlayerOneSetup,
            PlayerTwo = PlayerTwo != null
                ? new RoomSpectatorDto.RoomPlayerDto
                {
                    Id = PlayerTwo.Id,
                    Pseudo = PlayerTwo.Pseudo,
                }
                : null,
            PlayerTwoSetup = PlayerTwoSetup,
            StartedAt = StartedAt
        };
    }
    
    public record RoomFromListDto
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
        }
    }
    
    public record RoomOpponentDto
    {
        public required string Guid { get; init; }
        public ERoomState State { get; init; }
        public RoomPlayerDto? PlayerOne { get; init; }
        public RoomSetup? UserSetup { get; init; }
        public List<OffsetsWithHitDto>? UserFiredOffsets { get; init; }
        public RoomPlayerDto? PlayerTwo { get; init; }
        public List<OffsetsWithHitDto>? OpponentFiredOffsets { get; init; }
        public DateTime? StartedAt { get; init; }

        public record RoomPlayerDto
        {
            public long Id { get; init; }
            public string Pseudo { get; init; } = string.Empty;
        }
    }
    
    public record RoomSpectatorDto
    {
        public required string Guid { get; init; }
        public ERoomState State { get; init; }
        public RoomPlayerDto? PlayerOne { get; init; }
        public RoomSetup? PlayerOneSetup { get; init; }
        public RoomPlayerDto? PlayerTwo { get; init; }
        public RoomSetup? PlayerTwoSetup { get; init; }
        public DateTime? StartedAt { get; init; }

        public record RoomPlayerDto
        {
            public long Id { get; init; }
            public string Pseudo { get; init; } = string.Empty;
        }
    }
}

public record OffsetsDto
{
    public int XOffset { get; set; }
    public int YOffset { get; set; }
}

public record OffsetsWithHitDto
{
    public int XOffset { get; set; }
    public int YOffset { get; set; }
    public bool Hit { get; set; }
}

public record ShipOffsetsDto
{
    public string Guid { get; set; } = string.Empty;
    public string Orientation { get; set; } = string.Empty;
    public string Type { get; set; } = String.Empty;
    public OffsetsDto[] Offsets { get; set; } = [];
}