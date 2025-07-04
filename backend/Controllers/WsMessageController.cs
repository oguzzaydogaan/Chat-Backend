using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;
using Services;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("/ws/message")]
    public class WsMessageController : ControllerBase
    {
        public WsMessageController(MessageService messageService, JwtService jwtService, WsManager wsManager)
        {
            _messageService = messageService;
            _jwtService = jwtService;
            _wsManager = wsManager;
        }
        private readonly MessageService _messageService;
        private readonly JwtService _jwtService;
        private readonly WsManager _wsManager;
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string? token = HttpContext.Request.Query["accessToken"];

                var validatedToken = _jwtService.Validate(token) as JwtSecurityToken;
                int id = int.Parse(validatedToken!.Claims.FirstOrDefault(c => c.Type == "UserId")!.Value);

                await _wsManager.AddClient(id, webSocket);
                await _wsManager.ListenClient(id, validatedToken.ValidTo, _messageService);
            }
        }
    }
}
