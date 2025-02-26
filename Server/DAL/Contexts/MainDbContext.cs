namespace Server.DAL.Contexts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;
public class MainDbContext : IdentityDbContext<User, Role, long>
{
    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}