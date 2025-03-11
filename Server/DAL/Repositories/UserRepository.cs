using Microsoft.EntityFrameworkCore;
using Server.DAL.Contexts;
using Server.Models;

namespace Server.DAL.Repositories;

public class UserRepository
{
    private readonly MainDbContext _context;
    
    public UserRepository(MainDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserLeaderboardDto>> GetLeaderboard()
    {
        var users = await _context.Users.OrderByDescending(u => u.WinCount).Take(20).ToListAsync();
        return users.Select(u => new UserLeaderboardDto
        {
            Id = u.Id,
            Pseudo = u.Pseudo,
            WinCount = u.WinCount,
            Level = u.Level,
        }).ToList();
    }
    
    public record UserLeaderboardDto
    {
        public long Id { get; set; }
        public string Pseudo { get; set; } = string.Empty;
        public int WinCount { get; set; }
        public int Level { get; set; }
    }
}