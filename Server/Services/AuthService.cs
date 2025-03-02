using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Server.Models;
using Server.Services.Utils;

namespace Server.Services;

public class AuthService
{
    public static readonly string ClaimTypeToIdentifyUserOn = ClaimTypes.PrimarySid;

    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<User> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public ResponseService<string> GenerateUserJwt(User user)
    {
        var response = new ResponseService<string>();
        response.Data = GenerateTokenString(user);
        return response;
    }

    public async Task<ResponseService<User>> CreateOrUpdateUserFromNoyauSih(NoyauSihService.NoyauSihUser noyauUser)
    {
        var response = new ResponseService<User>();
        var userProperties = new User
        {
            IdRes = noyauUser.personne.id_res,
            Email = noyauUser.personne.courriel,
        };
        var user = await _userManager.FindByIdAsync(userProperties.Id.ToString());
        if (user == null)
        {
            await _userManager.CreateAsync(userProperties);
            response.Data = userProperties;
        }
        else
        {
            userProperties.Id = user.Id;
            await _userManager.UpdateAsync(userProperties);
            response.Data = user;
        }
        response.IsSuccess = true;
        return response;
    }

    private string GenerateTokenString(User user)
    {
        // Claims for Entity to retrieve when authorizing
        var claims = new List<Claim>
        {
            new (ClaimTypeToIdentifyUserOn, user.Id.ToString())
        };

        var securityKey = GetSymmetricJwtSecurityKey(_config);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var securityToken = new JwtSecurityToken(
            expires: GetJwtExpiration(),
            signingCredentials: signingCredentials,
            claims: claims
        );

        // Custom data
        securityToken.Payload["id"] = user.Id;
        securityToken.Payload["pseudo"] = user.Pseudo;

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    public static TokenValidationParameters GetAuthServiceTokenValidationParameters(IConfiguration config)
    {
        var securityKey = GetSymmetricJwtSecurityKey(config);
        return new TokenValidationParameters
        {
            IssuerSigningKey = securityKey,
            ValidateActor = false,
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypeToIdentifyUserOn
        };
    }

    private static DateTime GetJwtExpiration()
    {
        return DateTime.Now.AddHours(8);
    }

    private static SymmetricSecurityKey GetSymmetricJwtSecurityKey(IConfiguration config)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtPrivateKey(config)));
    }

    private static string GetJwtPrivateKey(IConfiguration config)
    {
        var key = config[$"{Program.ConfigurationProgram.JwtPrivateKey}"];

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JWT Private Key is missing or not configured.");
        }
        return key;
    }
}
