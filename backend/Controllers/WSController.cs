using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers
{
    [Route("/ws/message")]
    public class WSController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly WSListManager _wsClientListManager;
        private readonly ILogger<WSController> _logger;
        public WSController(JwtService jwtService, WSListManager wsClientListManager, ILogger<WSController> logger)
        {
            _jwtService = jwtService;
            _wsClientListManager = wsClientListManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {

                try
                {
                    string? token = HttpContext.Request.Query["accessToken"];
                    var validatedToken = _jwtService.Validate(token) as JwtSecurityToken;
                    int id = int.Parse(validatedToken!.Claims.FirstOrDefault(c => c.Type == "UserId")!.Value);

                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                    await _wsClientListManager.AddClient(id, webSocket, validatedToken.ValidTo);
                }
                catch (SecurityTokenArgumentException)
                {
                    return;
                }
                catch (SecurityTokenValidationException)
                {
                    return;
                }
                catch (SecurityTokenException)
                {
                    return;
                }
                catch (Exception ex) {
                    _logger.LogError(ex.Message);
                }
            }
        }
    }
}
