using AutoMapper;
using Exceptions;
using Microsoft.Extensions.Logging;
using Repositories.Entities;
using Services;
using Services.DTOs;
using Services.Helpers.LiveKit;
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
        private readonly CallService _callService;
        private readonly WSListManager _wsListManager;
        private readonly WSManager _wsManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ProcessMessage> _logger;

        public ProcessMessage(
            MessageService messageService,
            MessageReadService messageReadService,
            ChatService chatService,
            CallService callService,
            IMapper mapper,
            WSListManager wSClientListManager,
            ILogger<ProcessMessage> logger,
            WSManager wSManager
        )
        {
            _messageService = messageService;
            _messageReadService = messageReadService;
            _chatService = chatService;
            _callService = callService;
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
                ICollection<int> receivers = [];

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
                    receivers = chat.Users.Select(u => u.Id).ToList();
                }
                else if (messageJson.Type == RequestEventType.Message_Send)
                {
                    messageJson.Payload.Message!.Time = DateTime.UtcNow;
                    var res = new ResponseSocket_ForMessageDTO
                    {
                        Message = messageJson.Payload.Message,
                        Sender = messageJson.Sender
                    };
                    var send = _wsManager.SendMessageToUsersAsync(JsonSerializer.Serialize(res), messageJson.Receivers);
                    socketMessage.Type = ResponseEventType.Message_Saved;
                    socketMessage.Sender = messageJson.Sender;
                    var message = await _messageService.AddAsync(_mapper.Map<Message>(messageJson.Payload.Message));
                    receivers = message.Chat!.Users.Select(u => u.Id).ToList();
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
                    receivers = message.Chat!.Users.Select(u => u.Id).ToList();
                    socketMessage.Payload.Message = _mapper.Map<MessageWithSenderAndSeensDTO>(message);
                }
                else if (messageJson.Type == RequestEventType.Chat_Create)
                {
                    socketMessage.Type = ResponseEventType.Chat_Created;
                    socketMessage.Sender = messageJson.Sender;
                    var chat = await _chatService.AddAsync(messageJson.Payload.Chat, socketMessage.Sender);
                    receivers = chat.Users.Select(u => u.Id).ToList();
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
                    receivers = res.Item1.Users.Select(u => u.Id).ToList();
                    socketMessage.Payload.Message = _mapper.Map<MessageWithSenderAndSeensDTO>(res.Item2);
                    socketMessage.Payload.Chat = _mapper.Map<ChatWithUsersDTO>(res.Item1);
                }
                else if (messageJson.Type == RequestEventType.Call_Offer)
                {
                    var call = await _callService.AddAsync(messageJson.Payload.CreateCall);
                    var sfuToken = LiveKitHelper.GenerateToken(messageJson.Payload.CreateCall.ChatId.ToString(), messageJson.Sender.Id.ToString(), messageJson.Sender.Name);

                    var res = new ResponseSocket_SFUToken(_mapper.Map<CallDTO>(call), sfuToken);
                    res.Call.ChatId = messageJson.Payload.CreateCall.ChatId;
                    var resJson = JsonSerializer.Serialize(res);
                    await _wsManager.SendMessageToUserAsync(resJson, messageJson.Sender.Id);

                    socketMessage.Type = ResponseEventType.Call_Offered;
                    socketMessage.Payload.Call = _mapper.Map<CallDTO>(call);
                    socketMessage.Payload.Call.ChatId = messageJson.Payload.CreateCall.ChatId;
                    socketMessage.Sender = messageJson.Sender;
                    receivers = messageJson.Receivers;
                }
                else if (messageJson.Type == RequestEventType.Call_Accept)
                {
                    var sfuToken = LiveKitHelper.GenerateToken(messageJson.Payload.Call.ChatId.ToString(), messageJson.Sender.Id.ToString(), messageJson.Sender.Name);
                    var res = new ResponseSocket_SFUToken(messageJson.Payload.Call, sfuToken);
                    var resJson = JsonSerializer.Serialize(res);
                    await _wsManager.SendMessageToUserAsync(resJson, messageJson.Sender.Id);

                    await _callService.AcceptCallAsync(messageJson.Payload.Call.Id, messageJson.Receivers.Count);
                    socketMessage.Type = ResponseEventType.Call_Accepted;
                    socketMessage.Payload.Call = messageJson.Payload.Call;
                    socketMessage.Sender = messageJson.Sender;
                    receivers = messageJson.Receivers;
                }
                else if (messageJson.Type == RequestEventType.Call_End || messageJson.Type == RequestEventType.Call_Cancel || messageJson.Type == RequestEventType.Call_Reject)
                {
                    switch (messageJson.Type)
                    {
                        case RequestEventType.Call_Cancel:
                            await _callService.CancelCallAsync(messageJson.Payload.Call.Id, messageJson.Receivers.Count);
                            socketMessage.Type = ResponseEventType.Call_Cancelled;
                            break;
                        case RequestEventType.Call_Reject:
                            await _callService.RejectCallAsync(messageJson.Payload.Call.Id, messageJson.Receivers.Count);
                            socketMessage.Type = ResponseEventType.Call_Rejected;
                            break;
                        case RequestEventType.Call_End:
                            await _callService.EndCallAsync(messageJson.Payload.Call.Id, messageJson.Receivers.Count);
                            socketMessage.Type = ResponseEventType.Call_Ended;
                            break;
                    }

                    socketMessage.Payload.Call = messageJson.Payload.Call;
                    socketMessage.Sender = messageJson.Sender;
                    receivers = messageJson.Receivers;
                }
                else
                {
                    throw new Exception("Bad message type");
                }

                var json = JsonSerializer.Serialize(socketMessage);
                await _wsManager.SendMessageToUsersAsync(json, receivers);
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