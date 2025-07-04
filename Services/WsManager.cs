using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public class WsManager
    {

        public ConcurrentDictionary<int, WebSocket> _clients = new ConcurrentDictionary<int, WebSocket>();

        public async Task AddClient(int id, WebSocket client)
        {
            if (_clients.ContainsKey(id))
            {
                await RemoveClient(id);
            }
            _clients.TryAdd(id, client);
        }
        public async Task RemoveClient(int id)
        {
            var client = _clients[id];
            if (client != null)
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Kapatıldı.",
                        CancellationToken.None);
                }
                _clients.TryRemove(id, out var _);
            }
        }

        public async Task SendMessageToClients(byte[] bytes)
        {
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

        public async Task<byte[]> ConvertRequest(byte[] buffer, WebSocketReceiveResult receiveResult, MessageService _messageService)
        {
            var messageString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            SocketMessageDTO? messageJson = JsonSerializer.Deserialize<SocketMessageDTO>(messageString);
            ResponseSocketMessageDTO? socketMessage = new ResponseSocketMessageDTO();
            Message msg;
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
                    int mid = (int)messageJson!.Payload!.MessageID!;
                    socketMessage.Type = "Delete-Message";
                    msg = await _messageService.DeleteMessageAsync(mid);
                    socketMessage.Payload = msg.ToMessageForChatDTO();
                }
            }
            var json = JsonSerializer.Serialize(socketMessage);
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public async Task ListenClient(int id, DateTime validTo, MessageService _messageService)
        {
            var ws = _clients[id];
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult receiveResult;

            do
            {
                receiveResult = await ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
                if (validTo < DateTime.UtcNow)
                {
                    await RemoveClient(id);
                }               
                var bytes = await ConvertRequest(buffer, receiveResult, _messageService);
                await SendMessageToClients(bytes);
            }
            while (!receiveResult.CloseStatus.HasValue);

            await RemoveClient(id);

        }
    }
}