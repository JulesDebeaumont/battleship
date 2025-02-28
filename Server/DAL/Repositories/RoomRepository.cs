using Microsoft.EntityFrameworkCore;
using Server.DAL.Contexts;
using Server.Models;

namespace Server.DAL.Repositories;

public class RoomRepository
{
    private readonly MainDbContext _context;
    
    public RoomRepository(MainDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetRoomById(int roomId)
    {
        return await _context.Rooms.FindAsync(roomId);
    }

    public async Task<List<Room>> GetAvailableRooms()
    {
        return await _context.Rooms.Where(r => r.State == Room.ERoomState.Pending).ToListAsync();
    }

    public async Task UpdateRoomState(int roomId, Room.ERoomState state)
    {
        var room = await GetRoomById(roomId);
        if (room == null) return;
        room.State = state;
        await _context.SaveChangesAsync();
    }

    public async Task CreateRoom(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public void UpdateRoom(Room room)
    {
        _context.Rooms.Update(room);
    }
}   