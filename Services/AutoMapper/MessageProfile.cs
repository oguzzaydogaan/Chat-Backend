using AutoMapper;
using Repositories.Entities;
using Services.DTOs;

namespace Services.AutoMapper
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageForChatDTO>()
            .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.User));
            CreateMap<Message, GetAllMessagesResDTO>();
            CreateMap<RequestPayloadDTO, Message>();
        }
    }
}
