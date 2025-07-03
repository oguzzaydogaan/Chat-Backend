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
        public WsMessageController(ConcurrentDictionary<int, WebSocket> clients, MessageService messageService, IOptionsMonitor<JwtBearerOptions> authenticationOptions)
        {
            _clients = clients;
            _messageService = messageService;
            _authenticationOptions = authenticationOptions;
        }
        public ConcurrentDictionary<int, WebSocket> _clients;
        private readonly MessageService _messageService;
        private readonly IOptionsMonitor<JwtBearerOptions> _authenticationOptions;
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                int userId = int.Parse(HttpContext.Request.Query["userId"]!);
                string? token = HttpContext.Request.Query["accessToken"];
                try
                {
                    JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                    var parameters = _authenticationOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
                    jwtSecurityTokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);
                    if (_clients == null)
                        _clients = new();

                    else if (!_clients.ContainsKey(userId))
                        _clients[userId] = webSocket;

                    else
                    {
                        var oldSocket = _clients[userId];
                        if (oldSocket.State == WebSocketState.Open)
                            await oldSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Yeni bağlantı açıldı", CancellationToken.None);

                        _clients[userId] = webSocket;
                    }

                    await Echo(webSocket, _messageService, userId, validatedToken.ValidTo, _clients);
                }
                catch (SecurityTokenException)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.PolicyViolation,
                        "Token geçersiz.",
                        CancellationToken.None);
                }
                catch
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.InternalServerError,
                        "Bir hata oluştu",
                        CancellationToken.None);
                }
            }
        }
        private static async Task Echo(WebSocket webSocket, MessageService _messageService, int userId, DateTime validTime, ConcurrentDictionary<int, WebSocket> _clients)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult receiveResult;

            do
            {
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                if (validTime < DateTime.UtcNow)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.PolicyViolation,
                        "Token süresi doldu.",
                        CancellationToken.None);
                    if (_clients.TryRemove(userId, out var socket))
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                var messageString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                ResponseSocketMessageDTO? socketMessage = new ResponseSocketMessageDTO();
                Message msg = new Message();
                SocketMessageDTO? messageJson = null;
                messageJson = JsonSerializer.Deserialize<SocketMessageDTO>(messageString);
                if (messageJson?.Type == "Send-Message")
                {
                    socketMessage.Type = "Send-Message";
                    msg = await _messageService.AddMessageAsync(messageJson.Payload!.ToMessage()) ?? throw new Exception();
                    socketMessage.Payload = msg.ToMessageForChatDTO();
                }
                else
                {
                    if (messageJson != null)
                    {
                        int id = (int)messageJson!.Payload!.MessageID!;
                        socketMessage.Type = "Delete-Message";
                        msg = await _messageService.DeleteMessageAsync(id);
                        socketMessage.Payload = msg.ToMessageForChatDTO();
                    }
                }


                
                var json = JsonSerializer.Serialize(socketMessage);
                var bytes = Encoding.UTF8.GetBytes(json);

                foreach (var ws in _clients.Values)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
            }
            while (!receiveResult.CloseStatus.HasValue);

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);

            if (_clients.TryGetValue(userId, out var x))
            {

            }
        }
    }
}
