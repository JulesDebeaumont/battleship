using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Room
{
    public long Id { get; set; }
    
    [Required]
    public required long PlayerOneId { get; set; }
    
    [Required]
    public required long PlayerTwoId { get; set; }
    
    [Required]
    public required ERoomState State { get; set; }

    public ICollection<Move> Moves { get; set; } = [];
    
    [Column(TypeName = "jsonb")]
    public Setup? PlayerOneSetup { get; set; }
    
    [Column(TypeName = "jsonb")]
    public Setup? PlayerTwoSetup { get; set; }
        
    public User? PlayerOne { get; set; }
    
    public User? PlayerTwo { get; set; }

    public byte[] CreatedAt { get; set; } = [];
    
    public DateTime StartedAt { get; set; }
    
    public DateTime EndedAt { get; set; }

    public enum ERoomState
    {
        Playing,
        Pending,
        Archived
    }

    public class Setup
    {
        public List<ShipPlacement> Ships { get; set; } = [];

        public class ShipPlacement
        {
            public int[][] Positions { get; set; } = [];
        }
    }
}
