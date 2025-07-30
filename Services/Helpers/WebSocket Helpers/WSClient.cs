using AutoMapper;
using Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Entities;
using Services.DTOs;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Services
{
    public class WSClient
    {
        private readonly WSClientListManager _wsClientListManager;
        public WebSocket _client;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WSClient(WSClientListManager wsClientListManager, WebSocket client, IServiceScopeFactory serviceScopeFactory)
        {
            _wsClientListManager = wsClientListManager;
            _client = client;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task ListenClient(int id, DateTime validTo)
        {
            var buffer = new byte[1024 * 32];
            WebSocketReceiveResult receiveResult;

            do
            {
                receiveResult = await _client.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
                if (validTo < DateTime.UtcNow)
                {
                    throw new TokenExpiredException();
                }
                try
                {
                    var msg = await ConvertRequest(buffer, receiveResult);
                    if (msg.Bytes == null || msg.Users == null)
                    {
                        throw new ArgumentNullException("Msg error.");
                    }
                    await SendMessageToClients(msg.Bytes, msg.Users);
                }
                catch (ArgumentNullException ex)
                {
                    await SendErrorToClient(ex.Message);
                }
                catch (DbUpdateException)
                {
                    await SendErrorToClient("Database error.");
                }
                catch (NotSupportedException)
                {
                    await SendErrorToClient("JSON error.");
                }
                catch (ChatAlreadyExistException ex)
                {
                    await SendErrorToClient(ex.Message, ex.RedirectChatId);
                }
                catch (Exception ex)
                {
                    await SendErrorToClient(ex.Message);
                }
            }
            while (!receiveResult.CloseStatus.HasValue);

            await _wsClientListManager.RemoveClient(id, "Connection timed out. Please reconnect", _client);

        }

        public async Task<BytesWithUsersDTO> ConvertRequest(byte[] buffer, WebSocketReceiveResult receiveResult)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var _mapper = serviceScope.ServiceProvider.GetService<IMapper>();
            var messageService = serviceScope.ServiceProvider.GetService<MessageService>();
            var messageReadService = serviceScope.ServiceProvider.GetService<MessageReadService>();
            var chatService = serviceScope.ServiceProvider.GetService<ChatService>();
            if (messageService == null || chatService == null || _mapper == null || messageReadService == null)
            {
                throw new ArgumentNullException("An error occured");
            }
            var messageString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            RequestSocketMessageDTO? messageJson = JsonSerializer.Deserialize<RequestSocketMessageDTO>(messageString);
            if (string.IsNullOrEmpty(messageJson?.ToString()))
            {
                throw new ArgumentNullException("Message couldn't send");
            }
            ResponseSocketMessageDTO socketMessage = new();
            MessageWithUsersDTO? mWithUsers = new();

            if (messageJson.Type == "seen")
            {
                socketMessage.Type = "seen";
                socketMessage.Payload.MessageReads = [];
                var now = DateTime.UtcNow;
                foreach (var id in messageJson.Payload.Ids)
                {
                    socketMessage.Payload.MessageReads.Add(await messageReadService.AddWithoutSaveAsync(new MessageRead
                    {
                        MessageId = id,
                        UserName = messageJson.Sender.Name!,
                        UserId = messageJson.Sender.Id,
                        SeenAt = now,
                    }));
                }
                await messageReadService.SaveChangesAsync();
                socketMessage.Sender = messageJson.Sender;
                var chat = await chatService.GetChatWithUsersAsync(messageJson.Payload.ChatId);
                mWithUsers.Users = chat.Users;
            }
            else if (messageJson.Type == "Send-Message")
            {
                socketMessage.Type = "Send-Message";
                var message = await messageService.AddAsync(_mapper.Map<Message>(messageJson.Payload));
                mWithUsers.Message = message;
                mWithUsers.Users = message.Chat!.Users;
                socketMessage.Payload.Message = _mapper.Map<MessageForChatDTO>(mWithUsers.Message);
            }
            else if (messageJson.Type == "Delete-Message")
            {
                if (messageJson.Payload.MessageId == null)
                {
                    throw new ArgumentNullException("Message couldn't delete");
                }

                int mid = (int)messageJson.Payload.MessageId;
                socketMessage.Type = "Delete-Message";
                var message = await messageService.SoftDeleteAsync(mid, messageJson.Sender.Id);
                mWithUsers.Message = message;
                mWithUsers.Users = message.Chat!.Users;
                socketMessage.Payload.Message = _mapper.Map<MessageForChatDTO>(mWithUsers.Message);
            }
            else if (messageJson.Type == "New-Chat")
            {
                socketMessage.Type = "New-Chat";
                socketMessage.Sender = messageJson.Sender;
                var chat = await chatService.AddAsync(messageJson.Payload.Chat, socketMessage.Sender);
                mWithUsers.Users = chat.Users;
                socketMessage.Payload.Chat = _mapper.Map<SocketChatDTO>(chat);
            }
            else if (messageJson.Type == "User-Join")
            {
                socketMessage.Type = "User-Join";
                socketMessage.Sender = messageJson.Sender;
                var res = await chatService.AddUserAsync(messageJson.Payload.ChatId, messageJson.Payload.UserId, messageJson.Sender);
                mWithUsers.Users = res.Item1.Users;
                socketMessage.Payload.Message = _mapper.Map<MessageForChatDTO>(res.Item2);
                socketMessage.Payload.Chat = _mapper.Map<SocketChatDTO>(res.Item1);
            }

            var json = JsonSerializer.Serialize(socketMessage);
            var bytes = Encoding.UTF8.GetBytes(json);
            return new BytesWithUsersDTO { Bytes = bytes, Users = mWithUsers.Users };
        }


        public async Task SendErrorToClient(string ex, int id = -1)
        {
            ResponseSocketMessageDTO message = new()
            {
                Type = "Error",
                Payload =
                {
                    Error = ex,
                    Chat = new()
                    {
                        Id=id
                    }
                }
            };
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _client.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
        }


        public async Task SendMessageToClients(byte[] bytes, ICollection<User> users)
        {
            foreach (var user in users)
            {
                _wsClientListManager.Clients.TryGetValue(user.Id, out var ws);
                if (ws != null)
                {
                    if (ws._client.State == WebSocketState.Open)
                    {
                        await ws._client.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
            }
        }

        public async Task Close(string reason)
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    reason,
                    CancellationToken.None);
            }
        }
    }
}