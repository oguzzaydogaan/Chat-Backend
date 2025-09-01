using AutoMapper;
using Repositories.Entities;
using Services.DTOs;

namespace Services.AutoMapper
{
    public class CallProfile : Profile
    {
        public CallProfile()
        {
            CreateMap<Call, CreateCallReqDTO>();
            CreateMap<CreateCallReqDTO, Call>().AfterMap((src, dest, context) =>
            {
                var users = (List<User>)context.Items["Users"];
                dest.Callees = users;
            });
            CreateMap<Call, CallDTO>().AfterMap((src, dest, context) =>
            {
                var token = context.Items["SFUToken"].ToString();
                dest.SFUToken = token;
            });
        }
    }
}
