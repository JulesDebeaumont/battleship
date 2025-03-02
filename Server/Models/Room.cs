using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Room
{
    public int Id { get; set; }
    
    [Required]
    public required long PlayerOneId { get; set; }
    
    [Required]
    public long? PlayerTwoId { get; set; }

    public long? WinnerId { get; set; }
    
    [Required]
    public required ERoomState State { get; set; }

    [Column(TypeName = "jsonb")]
    [MaxLength(7000)]
    public string MovesJsonRaw { get; set; } = string.Empty;

    [NotMapped] public List<RoomMove> Moves { get; set; } = [];

    [Column(TypeName = "jsonb")]
    [MaxLength(2000)]
    public string PlayerOneSetupJsonRaw { get; set; } = string.Empty;
    
    [NotMapped]
    public RoomSetup? PlayerOneSetup { get; set; }

    [Column(TypeName = "jsonb")]
    [MaxLength(2000)]
    public string PlayerTwoSetupJsonRaw { get; set; } = string.Empty;
    
    [NotMapped]
    public RoomSetup? PlayerTwoSetup { get; set; }
        
    public User? PlayerOne { get; set; }
    
    public User? PlayerTwo { get; set; }

    public byte[] CreatedAt { get; set; } = [];
    
    public DateTime StartedAt { get; set; }
    
    public DateTime EndedAt { get; set; }

    public int LapCount { get; set; } = 1;
    
    [NotMapped]
    public bool PlayerOneIsReady => PlayerOneSetup?.Ships.Count > 0;

    [NotMapped]
    public bool PlayerTwoIsReady => PlayerTwoSetup?.Ships.Count > 0;
    
    [NotMapped]
    public bool BothPlayerReady => PlayerOneIsReady && PlayerTwoIsReady;

    [NotMapped]
    private const int LapCountSecond = 15;
    
    [NotMapped]
    private System.Timers.Timer Timer { get; } = new(1000 * LapCountSecond);

    [NotMapped] 
    public Action<int, int>? CallbackTimer { get; set; }

    public void StartTimer()
    {
        Timer.Enabled = true;
        Timer.AutoReset = false;
        Timer.Elapsed += (sender, args) =>
        {
            RunIntervalTimer();
            CallbackTimer?.Invoke(Id, LapCount);
        };
    }

    public int GetTimerInSeconds()
    {
        return (int)Timer.Interval * 1000;
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
    
    public enum ERoomState
    {
        Playing,
        Pending,
        Placing,
        Archived
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
}
