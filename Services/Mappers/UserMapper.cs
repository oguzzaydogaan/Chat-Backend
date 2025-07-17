using Repositories.Entities;
using Services.DTOs;

namespace Services.Mappers
{
    public static class UserMapper
    {
        public static UserDTO ToUserDTO(this User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name
            };
        }
        public static User ToUserEntity(this UserDTO userDto)
        {
            return new User
            {
                Id = userDto.Id,
                Name = userDto.Name
            };
        }
        public static User RegisterRequestDTOToUser(this RegisterRequestDTO registerRequest)
        {
            return new User
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };
        }
    }
}
