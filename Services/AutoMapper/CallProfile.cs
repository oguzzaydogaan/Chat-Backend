using AutoMapper;
using Repositories.Entities;
using Services.DTOs;

namespace Services.AutoMapper
{
    public class CallProfile : Profile
    {
        public CallProfile()
        {
            CreateMap<Call, CreateCallDTO>();
            CreateMap<CreateCallDTO, Call>();
        }
    }
}
