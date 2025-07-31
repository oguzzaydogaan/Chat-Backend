using AutoMapper;
using Repositories.Entities;
using Services.DTOs;

namespace Services.AutoMapper
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageWithSenderAndSeensDTO>()
            .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.User));
            CreateMap<Message, MessageDTO>();
            CreateMap<CreateMessageRequestDTO, Message>();
            CreateMap<Message, MessageRead>()
                .ForMember(dest=> dest.UserName, opt=>opt.MapFrom(src=>src.User!.Name))
                .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.Time));
        }
    }
}
