using AutoMapper;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;

namespace Services
{
    public class MessageReadService : BaseService<MessageRead, MessageDTO>
    {
        private readonly MessageReadRepository _messageReadRepository;
        private readonly UserRepository _userRepository;
        public MessageReadService(MessageReadRepository messageReadRepository, UserRepository userRepository, IMapper mapper)
        : base(mapper, messageReadRepository)
        {
            _messageReadRepository = messageReadRepository;
            _userRepository = userRepository;
        }


        public async Task<MessageRead> AddAsync(MessageRead messageRead)
        {
            messageRead = await _messageReadRepository.AddAsync(messageRead);
            return messageRead;
        }

        public async Task<List<UserDTO>> GetSeensAsync(int messageId)
        {
            var messageReads = await _messageReadRepository.GetByMessageIdAsync(messageId);
            var users = await _userRepository.GetByListOfIdsAsync(messageReads.Select(mr => mr.UserId).ToList());
            var dtos = users.Select(u => _mapper.Map<UserDTO>(u)).ToList();
            return dtos;
        }
    }
}
