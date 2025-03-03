using Server.DAL.Contexts;
using Server.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Server.DAL.Repositories;

public class RoomRepository
{
    private readonly MainDbContext _context;
    
    public RoomRepository(MainDbContext context)
    {
        _context = context;
    }
    
    public async Task CreateRoom(Room room)
    {
        room.MovesJsonRaw = JsonSerializer.Serialize(room.Moves);
        room.PlayerOneSetupJsonRaw = JsonSerializer.Serialize(room.PlayerOneSetup);
        room.PlayerTwoSetupJsonRaw = JsonSerializer.Serialize(room.PlayerTwoSetup);
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }
}   