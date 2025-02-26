using System.Text.Json;
using Server.Services.Utils;

namespace Server.Services;

public class NoyauSihService
{
  private readonly string CasUrl = "https://cas.chu-reims.fr";
  private readonly string NoyauSihUrl = "http://noyausih.domchurs.ad";
  private readonly string ResponseCasOk = "yes";
  private readonly string ApplicationCI = "CI0555"; // www-iias

  async public Task<ResponseService<string>> VerifyTicketFromCas(string ticket, string service)
  {
    var response = new ResponseService<string>();
    try
    {
      using (var client = new HttpClient())
      {
        var responseCas = await client.GetAsync($"{CasUrl}/validate?service={service}&ticket={ticket}");
        responseCas.EnsureSuccessStatusCode();
        var responseCasArray = (await responseCas.Content.ReadAsStringAsync()).Split("\n");
        if (responseCasArray?.First() != ResponseCasOk)
        {
          response.Errors.Add("CAS did not authorize");
          return response;
        }
        response.IsSuccess = true;
        response.Data = responseCasArray[1];
      }
      return response;
    }
    catch (Exception exception)
    {
      response.Errors.Add(exception.Message);
      return response;
    }
  }

  async public Task<ResponseService<NoyauSihUser>> GetUserFromNoyauSih(string userIdRes)
  {
    var response = new ResponseService<NoyauSihUser>();
    try
    {
      using (var client = new HttpClient())
      {
        var responseNoyau = await client.GetAsync($"{NoyauSihUrl}/iam/{userIdRes}/{ApplicationCI}/habilitations.json");
        responseNoyau.EnsureSuccessStatusCode();
        var responseNoyauSih = await responseNoyau.Content.ReadAsStringAsync();
        var noyauSihJson = JsonSerializer.Deserialize<NoyauSihUser>(responseNoyauSih);
        response.Data = noyauSihJson;
        response.IsSuccess = true;
      }

      return response;
    }
    catch (Exception exception)
    {
      response.Errors.Add(exception.Message);
      return response;
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