using System.Text.Json;
using Server.Services.Utils;

namespace Server.Services;

public class NoyauSihService
{
  private readonly string CasUrl = "https://cas.chu-reims.fr";
  private readonly string NoyauSihUrl = "http://noyausih.domchurs.ad";
  private readonly string ResponseCasOk = "yes";
  private readonly string ApplicationCI = "CI0555"; // www-iias

  public async Task<string> VerifyTicketFromCas(string ticket, string service)
  {
    try
    {
      using var client = new HttpClient();
      var responseCas = await client.GetAsync($"{CasUrl}/validate?service={service}&ticket={ticket}");
      responseCas.EnsureSuccessStatusCode();
      var responseCasArray = (await responseCas.Content.ReadAsStringAsync()).Split("\n");
      if (responseCasArray.First() != ResponseCasOk)
      {
        throw new ServiceError("CAS did not authorize", ServiceError.EServiceErrorType.NotAuthorized);
      }
      return responseCasArray[1];
    }
    catch (Exception exception)
    {
      throw new ServiceError("An error has occured", ServiceError.EServiceErrorType.InternalError, exception);
    }
  }

  public async Task<NoyauSihUser> GetUserFromNoyauSih(string userIdRes)
  {
    try
    {
      using var client = new HttpClient();
      var responseNoyau = await client.GetAsync($"{NoyauSihUrl}/iam/{userIdRes}/{ApplicationCI}/habilitations.json");
      responseNoyau.EnsureSuccessStatusCode();
      var responseNoyauSih = await responseNoyau.Content.ReadAsStringAsync();
      var noyauSihJson = JsonSerializer.Deserialize<NoyauSihUser>(responseNoyauSih);
      return noyauSihJson!;
    }
    catch (Exception exception)
    {
      throw new ServiceError("An error has occured", ServiceError.EServiceErrorType.InternalError, exception);
    }
  }

  public record NoyauSihUser
  {
    public required NoyauSihUserPersonne personne { get; set; }
    public required object profils { get; set; }

    public record NoyauSihUserPersonne
    {
      public string courriel { get; set; } = string.Empty;
      public string nom { get; set; } = string.Empty;
      public string prenom { get; set; } = string.Empty;
      public required string id_res { get; set; }

    }
  }
}