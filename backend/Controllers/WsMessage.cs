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
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("/ws/message")]
    public class WsMessage : ControllerBase
    {
        public WsMessage(ConcurrentDictionary<int, ConcurrentDictionary<int, WebSocket>> clients, MessageService messageService, IOptionsMonitor<JwtBearerOptions> authenticationOptions)
        {
            _clients = clients;
            _messageService = messageService;
            _authenticationOptions = authenticationOptions;
        }
        public ConcurrentDictionary<int, ConcurrentDictionary<int, WebSocket>> _clients;
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
                try
                {
                    JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                    var parameters = _authenticationOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
                    jwtSecurityTokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);
                    if (_clients == null)
                        _clients = new();

                    if (!_clients.ContainsKey(chatId))
                        _clients[chatId] = new();

                    if (_clients[chatId].ContainsKey(userId))
                    {
                        var oldSocket = _clients[chatId][userId];
                        if (oldSocket.State == WebSocketState.Open)
                            await oldSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Yeni bağlantı açıldı", CancellationToken.None);

                        _clients[chatId][userId] = webSocket;
                    }
                    else
                    {
                        _clients[chatId].TryAdd(userId, webSocket);
                    }

                    await Echo(webSocket, _messageService, chatId, userId, validatedToken.ValidTo, _clients);
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
        private static async Task Echo(WebSocket webSocket, MessageService _messageService, int chatId, int userId, DateTime validTime, ConcurrentDictionary<int, ConcurrentDictionary<int, WebSocket>> _clients)
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
                    if (_clients.TryGetValue(chatId, out var chatClients))
                    {
                        chatClients.TryRemove(userId, out _);

                        if (chatClients.IsEmpty)
                            _clients.TryRemove(chatId, out _);
                    }
                    return;
                }

                var messageJson = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                Message? message = null;
                if (messageJson.Contains("delete/"))
                {
                    int id = int.Parse(messageJson.Split("/")[1]);
                    message = await _messageService.DeleteMessageAsync(id);
                }
                else
                {
                    message = JsonSerializer.Deserialize<Message>(messageJson);

                    if (message != null)
                        await _messageService.AddMessageAsync(message);
                }          
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
            }
            while (!receiveResult.CloseStatus.HasValue);

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);

            if (_clients.TryGetValue(chatId, out var chatClientsInner))
            {
                chatClientsInner.TryRemove(userId, out _);

                if (chatClientsInner.IsEmpty)
                    _clients.TryRemove(chatId, out _);
            }
        }
    }
}
