using Microsoft.AspNetCore.Mvc;
using Services;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers
{
    [Route("/ws/message")]
    public class WSController : ControllerBase
    {
        public WSController(JwtService jwtService, WSClientListManager wsClientListManager)
        {
            _jwtService = jwtService;
            _wsClientListManager = wsClientListManager;
        }
        private readonly JwtService _jwtService;
        private readonly WSClientListManager _wsClientListManager;

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string token = HttpContext.Request.Query["accessToken"];

                var validatedToken = _jwtService.Validate(token) as JwtSecurityToken;
                int id = int.Parse(validatedToken!.Claims.FirstOrDefault(c => c.Type == "UserId")!.Value);

                await _wsClientListManager.AddClient(id, webSocket, validatedToken.ValidTo);
            }
        }
    }
}
