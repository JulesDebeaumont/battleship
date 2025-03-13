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
            var userIdRes = await _noyauService.VerifyTicketFromCas(dtoIn.CasTicket, dtoIn.Service);
            var noyauSihUser = await _noyauService.GetUserFromNoyauSih(userIdRes);
            var authUser = await _authService.CreateOrUpdateUserFromNoyauSih(noyauSihUser);
            var jwt = _authService.GenerateUserJwt(authUser);
            return Ok(new { token = jwt });
        }

        // Dev only
        /*
        [HttpPost("fake-login")]
        public async Task<ActionResult> FakeLogin(FakeLoginDtoIn dtoIn)
        {
            var fakeUserFromFakeNoay = await _noyauService.GetFakeUserFromNoyauSih(dtoIn.IdRes);
            var authUser = await _authService.CreateOrUpdateUserFromNoyauSih(fakeUserFromFakeNoay);
            var jwt = _authService.GenerateUserJwt(authUser);
            return Ok(new { token = jwt });
        }
        */
        
    }

    public record LoginDtoIn
    {
        public required string CasTicket { get; set; }
        public required string Service { get; set; }
    }
    
    public record FakeLoginDtoIn
    {
        public required string IdRes { get; set; }
    }
    
}