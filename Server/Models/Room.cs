using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Room
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Guid { get; set; }
    
    [Required]
    public required long PlayerOneId { get; set; }
    
    public long? PlayerTwoId { get; set; }

    public long? WinnerId { get; set; }
    
    [Required]
    public required ERoomState State { get; set; }

    [Column(TypeName = "jsonb")]
    [MaxLength(7000)]
    public string MovesJsonRaw { get; set; } = string.Empty;


    [Column(TypeName = "jsonb")]
    [MaxLength(2000)]
    public string PlayerOneSetupJsonRaw { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    [MaxLength(2000)]
    public string PlayerTwoSetupJsonRaw { get; set; } = string.Empty;
    
    public User? PlayerOne { get; set; }
    
    public User? PlayerTwo { get; set; }

    [Timestamp]
    public byte[]? CreatedAt { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? EndedAt { get; set; }

    public int LapCount { get; set; } = 1;
}

public enum ERoomState
{
    Playing,
    Pending,
    Placing,
    Archived
}