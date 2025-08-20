using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;

namespace backend.Controllers
{
    [Route("/call")]
    public class CallController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<CallController> _logger;
        public CallController(JwtService jwtService, ILogger<CallController> logger)
        {
            _jwtService = jwtService;
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
                    int toId = int.Parse(HttpContext.Request.Query["toId"]);

                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                    var buffer = new byte[1024 * 4];
                    await using var fs = new FileStream("recorded_audio.webm", FileMode.Create, FileAccess.Write);
                    WebSocketReceiveResult receiveResult;
                    while (true)
                    {

                        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        await fs.WriteAsync(buffer, 0, receiveResult.Count);

                        if (receiveResult.CloseStatus.HasValue)
                        {
                            break;
                        }
                    }

                    await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
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
