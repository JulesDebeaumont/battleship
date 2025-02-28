using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Server.Services;
using Server.DAL.Contexts;
using Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Server.DAL.Repositories;
using Server.Hubs;

namespace Server;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    RunBeforeStart(builder);
    RegisterServices(builder);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
      var services = scope.ServiceProvider;
      var dbContext = services.GetRequiredService<MainDbContext>();
      dbContext.Database.Migrate();
    }


    // app.UseHttpsRedirection();
    app.UseCors(app.Environment.EnvironmentName);
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
      ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<RoomHub>("/api/ws/rooms");
    app.Run();
  }

  private static void RunBeforeStart(WebApplicationBuilder builder)
  {
    ConfigurationProgram.EnsureSecretsAreDefined(builder.Configuration);
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
  }

  private static void RegisterServices(WebApplicationBuilder builder)
  {
    builder.Services.AddSignalR();
    builder.Services.AddControllers();
    builder.Services.AddDbContext<MainDbContext>(options =>
        options.UseNpgsql(builder.Configuration[ConfigurationProgram.ConnectionString]));
    builder.Services.AddTransient<AuthService>();
    builder.Services.AddTransient<NoyauSihService>();
    builder.Services.AddScoped<RoomRepository>();
    builder.Services.AddScoped<MoveRepository>();
    builder.Services.AddSingleton<RoomManager>();

    builder.Services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<MainDbContext>()
                .AddDefaultTokenProviders();

    builder.Services.AddAuthentication(options =>
        {
          options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
          options.TokenValidationParameters = AuthService.GetAuthServiceTokenValidationParameters(builder.Configuration);
        });
    builder.Services.AddAuthorization();
    builder.Services.AddCors(options =>
        {
          options.AddPolicy("Production", builderPolicy =>
              {
                // TODO
              });
          options.AddPolicy("Development", builderPolicy =>
              {
                builderPolicy.WithOrigins("http://localhost:9000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
              });
        });
  }
  
  private static class ConfigurationProgram
  {
    private const string JwtPrivateKey = "JwtPrivateKey";
    public const string ConnectionString = "ConnectionString";
  
    public static void EnsureSecretsAreDefined(IConfiguration configuration)
    {

     
      var secretsToCheck = new []
      {
        JwtPrivateKey,
        ConnectionString,
      };
      foreach (var secret in secretsToCheck)
      {
        if (configuration[secret] is null)
        {
          throw new Exception($"Secret '{secret}' is not defined!");
        }
      }

    }
  }
}
