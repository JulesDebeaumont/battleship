using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Server.Models;

public class Room
{
    public long Id { get; set; }
    
    [Required]
    public required long PlayerOneId { get; set; }
    
    [Required]
    public long? PlayerTwoId { get; set; }
    
    [Required]
    public required ERoomState State { get; set; }

    [Column(TypeName = "jsonb")]
    public List<RoomMove> Moves { get; set; } = [];
    
    [Column(TypeName = "jsonb")]
    public RoomSetup? PlayerOneSetup { get; set; }
    
    [Column(TypeName = "jsonb")]
    public RoomSetup? PlayerTwoSetup { get; set; }
        
    public User? PlayerOne { get; set; }
    
    public User? PlayerTwo { get; set; }

    public byte[] CreatedAt { get; set; } = [];
    
    public DateTime StartedAt { get; set; }
    
    public DateTime EndedAt { get; set; }

    public int LapCount { get; set; } = 1;
    
    [NotMapped]
    public bool PlayerOneIsReady => PlayerOneSetup != null;

    [NotMapped]
    public bool PlayerTwoIsReady => PlayerTwoSetup != null;

    [NotMapped]
    private const int LapCountSecond = 15;
    
    [NotMapped]
    private System.Timers.Timer Timer { get; } = new(1000 * LapCountSecond);

    public void StartTimer(Action<int> callBack)
    {
        Timer.Enabled = true;
        Timer.AutoReset = false;
        Timer.Elapsed += (sender, args) =>
        {
            RunIntervalTimer();
            callBack.Invoke(LapCount);
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
    
    public enum ERoomState
    {
        Playing,
        Pending,
        Archived
    }

    public class RoomSetup
    {
        public List<Ship> Ships { get; set; } = [];
        public Dictionary<int, int>[] FiredOffsets { get; set; } = [];
        public bool Dead = false;

        public class Ship
        {
            public Tuple<int, int>[] Positions { get; set; } = [];
            public Tuple<int, int>[] Hits { get; set; } = [];
            public bool IsDrawned = false;

            public void TryHit(int xOffset, int yOffset)
            {
                foreach (var position in Positions)
                {
                    if (position.Item1 == xOffset && position.Item2 == yOffset)
                    {
                        var hit = new Tuple<int, int>(xOffset, yOffset);
                        Hits = Hits.Append(hit).ToArray();
                    }
                }
                if (Positions.Length == Hits.Length)
                {
                    IsDrawned = true;
                }
            }
        }
        
        public void FireOffset(int xOffset, int yOffset)
        {
            foreach (var ship in Ships)
            {
                ship.TryHit(xOffset, yOffset);
            }
            if (Ships.FindIndex(s => !s.IsDrawned) == -1)
            {
                Dead = true;
            }
        }
    }
    
    public class RoomMove
    {
        public long Id { get; set; }
    
        public int Lap { get; set; }
    
        public long PlayerId { get; set; }
    
        public int XOffset { get; set; }
    
        public int YOffset { get; set; }
    
        public bool Hit { get; set; }
    }
}
