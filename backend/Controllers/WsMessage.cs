using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("/ws/message")]
    public class WsMessage : ControllerBase
    {
        public WsMessage(Dictionary<int, Dictionary<int, WebSocket>> clients, ChatService chatService, MessageService messageService, IOptionsMonitor<JwtBearerOptions> authenticationOptions)
        {
            _clients = clients;
            _chatService = chatService;
            _messageService = messageService;
            _authenticationOptions = authenticationOptions;
        }
        public static Dictionary<int, Dictionary<int, WebSocket>> _clients;
        private readonly ChatService _chatService;
        private readonly MessageService _messageService;
        private readonly IOptionsMonitor<JwtBearerOptions> _authenticationOptions;
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                int chatId = int.Parse(HttpContext.Request.Query["chatId"]!);
                int userId = int.Parse(HttpContext.Request.Query["userId"]!);
                string? token = HttpContext.Request.Query["accessToken"];

                JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                var parameters = _authenticationOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
                var x = jwtSecurityTokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);
                if (x == null && x?.Identity?.IsAuthenticated != true)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError,"Yeni oturum gerekli.",CancellationToken.None);
                }
                if (!_clients.ContainsKey(chatId))
                {
                    _clients[chatId] = new Dictionary<int, WebSocket>();
                }

                if (_clients[chatId].ContainsKey(userId))
                {
                    var oldSocket = _clients[chatId][userId];
                    if (oldSocket.State == WebSocketState.Open)
                        await oldSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Yeni bağlantı açıldı", CancellationToken.None);

                    _clients[chatId][userId] = webSocket;
                }
                else
                {
                    _clients[chatId].Add(userId, webSocket);
                }

                await Echo(webSocket, _messageService, chatId, userId, validatedToken.ValidTo);
            }
            else
            {
                Console.WriteLine("Bu endpoint sadece WebSocket istekleri için kullanılabilir.");
            }
        }
        private static async Task Echo(WebSocket webSocket, MessageService _messageService, int chatId, int userId, DateTime validTime)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                if (validTime < DateTime.UtcNow)
                {
                    break;
                }
                var messageJson = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                var message = JsonSerializer.Deserialize<Message>(messageJson);

                if (message != null)
                    await _messageService.AddMessageAsync(message);
                var newMessage = message!.ToMessageForChatDTO();
                var json = JsonSerializer.Serialize(newMessage);
                var bytes = Encoding.UTF8.GetBytes(json);
                foreach (var ws in _clients[chatId].Values)
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
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
            _clients[chatId].Remove(userId);
            if (_clients[chatId].Count == 0)
                _clients.Remove(chatId);
        }
    }
}
