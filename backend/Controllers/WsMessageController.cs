using Microsoft.AspNetCore.Mvc;
using Services;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers
{
    [Route("/ws/message")]
    public class WsMessageController : ControllerBase
    {
        public WsMessageController(JwtService jwtService, WSClientListManager wsManager, MessageService messageService)
        {
            _jwtService = jwtService;
            _wsManager = wsManager;
            _messageService = messageService;
        }
        private readonly JwtService _jwtService;
        private readonly WSClientListManager _wsManager;
        private readonly MessageService _messageService;

        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string token = HttpContext.Request.Query["accessToken"];

                var validatedToken = _jwtService.Validate(token) as JwtSecurityToken;
                int id = int.Parse(validatedToken!.Claims.FirstOrDefault(c => c.Type == "UserId")!.Value);

                await _wsManager.AddClient(id, webSocket, validatedToken.ValidTo, _messageService);
            }
        }
    }
}
