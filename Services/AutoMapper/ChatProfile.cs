using AutoMapper;
using Repositories.Entities;
using Services.DTOs;

namespace Services.AutoMapper
{
    public class ChatProfile : Profile
    {
        public ChatProfile()
        {
            CreateMap<Chat, ChatDTO>();
            CreateMap<Chat, ChatWithNotSeensDTO>()
                .AfterMap((src, dest, context) =>
                {
                    var count = (int)context.Items["Count"];
                    dest.Count = count;
                });
            CreateMap<Chat, ChatWithMessagesDTO>();
            CreateMap<CreateChatRequestDTO, Chat>()
                .AfterMap((src, dest, context) =>
                {
                    var users = (List<User>)context.Items["Users"];
                    dest.Users = users;
                    if (users.Count == 2)
                    {
                        dest.Name = string.Join(", ", users.Select(u => u.Name));
                    }
                });
            CreateMap<CreateChatResponseDTO, Chat>();
            CreateMap<Chat, SocketChatDTO>();
        }
    }
}
