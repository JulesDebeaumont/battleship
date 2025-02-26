using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly NoyauSihService _noyauService;

        public AuthController(AuthService authService, NoyauSihService noyauService)
        {
            _authService = authService;
            _noyauService = noyauService;
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDtoIn dtoIn)
        {
            var noyauServiceResponseCas = await _noyauService.VerifyTicketFromCas(dtoIn.CasTicket, dtoIn.Service);
            if (!noyauServiceResponseCas.IsSuccess)
            {
                return Unauthorized(noyauServiceResponseCas.Errors);
            }
            var noyauServiceReponseUser = await _noyauService.GetUserFromNoyauSih(noyauServiceResponseCas.Data);
            if (!noyauServiceReponseUser.IsSuccess)
            {
                return Unauthorized(noyauServiceReponseUser.Errors);
            }
            var authServiceResponseUser = await _authService.CreateOrUpdateUserFromNoyauSih(noyauServiceReponseUser.Data);
            if (!authServiceResponseUser.IsSuccess)
            {
                return Unauthorized(authServiceResponseUser.Errors);
            }
            var authServiceResponseJwt = _authService.GenerateUserJwt(authServiceResponseUser.Data);
            return Ok(authServiceResponseJwt.Data);
        }
    }

    public record LoginDtoIn
    {
        public required string CasTicket { get; set; }
        public required string Service { get; set; }
    }
}