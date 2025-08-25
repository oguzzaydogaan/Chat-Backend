using AutoMapper;
using Exceptions;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;

namespace Services
{
    public class CallService : BaseService<Call, CallDTO>
    {
        private readonly CallRepository _callRepository;
        private readonly UserRepository _userRepository;

        public CallService(CallRepository callRepository, UserRepository userRepository, IMapper mapper)
        : base(mapper, callRepository)
        {
            _callRepository = callRepository;
            _userRepository = userRepository;
        }

        public async Task<Call> AddAsync(CreateCallDTO dto)
        {
            var caller = await _userRepository.GetByIdAsync(dto.CallerId) ?? throw new UsersNotFoundException();
            var callee = await _userRepository.GetByIdAsync(dto.CalleeId) ?? throw new UsersNotFoundException();

            return await _callRepository.AddAsync(_mapper.Map<Call>(dto));
        }

        public async Task CancelCallAsync(int id)
        {
            var call = await _callRepository.GetByIdAsync(id) ?? throw new UIException("Call not found.");
            call.AnswerType = CallAnswerType.Cancelled;
            await _callRepository.SaveChangesAsync();
        }

        public async Task AcceptCallAsync(int id)
        {
            var call = await _callRepository.GetByIdAsync(id) ?? throw new UIException("Call not found.");
            call.AnswerType = CallAnswerType.Accepted;
            call.StartTime = DateTime.UtcNow;
            await _callRepository.SaveChangesAsync();
        }

        public async Task RejectCallAsync(int id)
        {
            var call = await _callRepository.GetByIdAsync(id) ?? throw new UIException("Call not found.");
            call.AnswerType = CallAnswerType.Rejected;
            await _callRepository.SaveChangesAsync();
        }

        public async Task EndCallAsync(int id)
        {
            var call = await _callRepository.GetByIdAsync(id) ?? throw new UIException("Call not found.");
            call.EndTime = DateTime.UtcNow;
            call.DurationInSeconds = (int)(call.EndTime - call.StartTime).TotalSeconds;
            await _callRepository.SaveChangesAsync();
        }

        public async Task<(ResponseSocketDTO,ICollection<int>)?> CheckAndCloseActiveCallAsync(int userId)
        {
            var call = await _callRepository.GetActiveByUserIdAsync(userId);
            if (call != null)
            {
                ResponseSocketDTO socketMessage = new()
                {
                    Payload = new ResponsePayloadDTO
                    {
                        Call = new CallOfferDTO
                        {
                            CallId = call.Id
                        }
                    },
                    Sender = new UserDTO { Id = userId }
                };
                ICollection<int> recievers = call.CallerId == userId ? [call.CalleeId] : [call.CallerId];
                if (call.AnswerType == CallAnswerType.None)
                {
                    if (call.CallerId == userId)
                    {
                        call.AnswerType = CallAnswerType.Cancelled;
                        socketMessage.Type = ResponseEventType.Call_Cancelled;
                    }
                    else
                    {
                        call.AnswerType = CallAnswerType.Rejected;
                        socketMessage.Type = ResponseEventType.Call_Rejected;
                    }
                }
                else if (call.AnswerType == CallAnswerType.Accepted)
                {
                    call.EndTime = DateTime.UtcNow;
                    call.DurationInSeconds = (int)(call.EndTime - call.StartTime).TotalSeconds;
                    socketMessage.Type = ResponseEventType.Call_Ended;
                }
                await _callRepository.SaveChangesAsync();
                return (socketMessage, recievers);
            }
            return null;
        }
    }
}
