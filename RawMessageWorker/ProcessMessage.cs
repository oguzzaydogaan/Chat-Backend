using AutoMapper;
using Exceptions;
using Microsoft.Extensions.Logging;
using Repositories.Entities;
using Services;
using Services.DTOs;
using Services.Helpers.WebSocket_Helpers;
using System.Text;
using System.Text.Json;

namespace RawMessageWorker
{
    public class ProcessMessage
    {
        private readonly MessageService _messageService;
        private readonly MessageReadService _messageReadService;
        private readonly ChatService _chatService;
        private readonly WSListManager _wsListManager;
        private readonly WSManager _wsManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ProcessMessage> _logger;

        public ProcessMessage(
            MessageService messageService,
            MessageReadService messageReadService,
            ChatService chatService,
            IMapper mapper,
            WSListManager wSClientListManager,
            ILogger<ProcessMessage> logger,
            WSManager wSManager
        )
        {
            _messageService = messageService;
            _messageReadService = messageReadService;
            _chatService = chatService;
            _mapper = mapper;
            _logger = logger;
            _wsListManager = wSClientListManager;
            _wsManager = wSManager;
        }

        public async Task ProcessMessageAsync(string result)
        {
            RequestSocketDTO? messageJson = new();
            try
            {
                messageJson = JsonSerializer.Deserialize<RequestSocketDTO>(result);
                if (string.IsNullOrEmpty(messageJson?.ToString()))
                {
                    throw new ArgumentNullException("Message couldn't send");
                }
                ResponseSocketDTO socketMessage = new();
                ICollection<int> recievers = [];

                if (messageJson.Type == RequestEventType.Message_See)
                {
                    socketMessage.Type = ResponseEventType.Message_Seen;
                    socketMessage.Sender = messageJson.Sender;
                    socketMessage.Payload.MessageReads = [];
                    var now = DateTime.UtcNow;
                    if (messageJson.Payload.Ids == null)
                    {
                        throw new ArgumentNullException("Ids cannot be null");
                    }
                    foreach (var id in messageJson.Payload.Ids)
                    {
                        socketMessage.Payload.MessageReads.Add(await _messageReadService.AddWithoutSaveAsync(new MessageRead
                        {
                            MessageId = id,
                            UserName = messageJson.Sender.Name!,
                            UserId = messageJson.Sender.Id,
                            SeenAt = now,
                        }));
                    }
                    await _messageReadService.SaveChangesAsync();
                    socketMessage.Sender = messageJson.Sender;
                    if (messageJson.Payload.Id == null)
                    {
                        throw new ArgumentNullException("Id cannot be null");
                    }
                    var chat = await _chatService.GetChatWithUsersAsync((int)messageJson.Payload.Id);
                    recievers = chat.Users.Select(u => u.Id).ToList();
                }
                else if (messageJson.Type == RequestEventType.Message_Send)
                {  
                    
                    var res = new ResponseSocket_ForMessageDTO
                    {
                        Message = messageJson.Payload.Message,
                        Sender = messageJson.Sender
                    };
                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(res));
                    var send = _wsManager.SendMessageToUsersAsync(body, messageJson.Recievers);
                    socketMessage.Type = ResponseEventType.Message_Saved;
                    socketMessage.Sender = messageJson.Sender;
                    var message = await _messageService.AddAsync(_mapper.Map<Message>(messageJson.Payload.Message));
                    recievers = message.Chat!.Users.Select(u=> u.Id).ToList();
                    socketMessage.Payload.Message = _mapper.Map<MessageWithSenderAndSeensDTO>(message);
                    socketMessage.Payload.Message.LocalId = messageJson.Payload.Message!.LocalId;
                    await send;
                }
                else if (messageJson.Type == RequestEventType.Message_Delete)
                {
                    if (messageJson.Payload.Id == null)
                    {
                        throw new ArgumentNullException("Message couldn't delete");
                    }

                    int mid = (int)messageJson.Payload.Id;
                    socketMessage.Type = ResponseEventType.Message_Deleted;
                    socketMessage.Sender = messageJson.Sender;
                    var message = await _messageService.SoftDeleteAsync(mid, messageJson.Sender.Id);
                    recievers = message.Chat!.Users.Select(u => u.Id).ToList();
                    socketMessage.Payload.Message = _mapper.Map<MessageWithSenderAndSeensDTO>(message);
                }
                else if (messageJson.Type == RequestEventType.Chat_Create)
                {
                    socketMessage.Type = ResponseEventType.Chat_Created;
                    socketMessage.Sender = messageJson.Sender;
                    var chat = await _chatService.AddAsync(messageJson.Payload.Chat, socketMessage.Sender);
                    recievers = chat.Users.Select(u => u.Id).ToList();
                    socketMessage.Payload.Chat = _mapper.Map<ChatWithUsersDTO>(chat);
                }
                else if (messageJson.Type == RequestEventType.Chat_AddUser)
                {
                    socketMessage.Type = ResponseEventType.Chat_UserAdded;
                    socketMessage.Sender = messageJson.Sender;
                    if (messageJson.Payload.Message == null)
                    {
                        throw new ArgumentNullException("Informations cannot be null");
                    }
                    var res = await _chatService.AddUserAsync(messageJson.Payload.Message.ChatId, messageJson.Payload.Message.UserId, messageJson.Sender);
                    recievers = res.Item1.Users.Select(u => u.Id).ToList();
                    socketMessage.Payload.Message = _mapper.Map<MessageWithSenderAndSeensDTO>(res.Item2);
                    socketMessage.Payload.Chat = _mapper.Map<ChatWithUsersDTO>(res.Item1);
                }
                else
                {
                    throw new Exception("Bad message type");
                }

                var json = JsonSerializer.Serialize(socketMessage);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _wsManager.SendMessageToUsersAsync(bytes, recievers);
            }
            catch (ChatAlreadyExistException ex)
            {
                var webSocket = _wsListManager.FindClient(messageJson!.Sender.Id);
                await _wsManager.SendErrorAsync(webSocket, ex.Message, ex.RedirectChatId);
            }
            catch (UIException ex)
            {
                var webSocket = _wsListManager.FindClient(messageJson!.Sender.Id);
                await _wsManager.SendErrorAsync(webSocket, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
                var webSocket = _wsListManager.FindClient(messageJson!.Sender.Id);
                await _wsManager.SendErrorAsync(webSocket, "Something went wrong.");
            }
        }
    }
}