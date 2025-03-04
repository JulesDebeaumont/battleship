using Server.DAL.Contexts;
using Server.Models;
using Server.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Server.DAL.Repositories;

public class RoomRepository
{
    private readonly MainDbContext _context;
    
    public RoomRepository(MainDbContext context)
    {
        _context = context;
    }
    
    public async Task CreateRoom(RoomAvailable roomAvailable)
    {
        var room = new Room
        {
            Guid = roomAvailable.Guid,
            PlayerOneId = roomAvailable.PlayerOneId,
            PlayerTwoId = roomAvailable.PlayerTwoId,
            WinnerId = roomAvailable.WinnerId,
            State = roomAvailable.State,
            LapCount = roomAvailable.LapCount,
            StartedAt = roomAvailable.StartedAt,
            EndedAt = roomAvailable.EndedAt,
            MovesJsonRaw = JsonSerializer.Serialize(roomAvailable.Moves),
            PlayerOneSetupJsonRaw = JsonSerializer.Serialize(roomAvailable.PlayerOneSetup),
            PlayerTwoSetupJsonRaw = JsonSerializer.Serialize(roomAvailable.PlayerTwoSetup)
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }
}   