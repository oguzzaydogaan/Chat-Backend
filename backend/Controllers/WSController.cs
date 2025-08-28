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
        private readonly WSListManager _wsListManager;
        private readonly ILogger<WSController> _logger;
        public WSController(JwtService jwtService, WSListManager wsListManager, ILogger<WSController> logger)
        {
            _jwtService = jwtService;
            _wsListManager = wsListManager;
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

                    await _wsListManager.AddClient(id, webSocket, validatedToken.ValidTo);
                }
                catch (SecurityTokenArgumentException ex)
                {
                    _logger.LogError($"Invalid token argument: {ex.Message}");
                }
                catch (SecurityTokenValidationException ex)
                {
                    _logger.LogError($"Token validation failed: {ex.Message}");
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogError($"Token is invalid or expired: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }
    }
}
