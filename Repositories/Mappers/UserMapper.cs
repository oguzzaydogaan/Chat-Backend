using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.DTOs;
using Repositories.Entities;

namespace Repositories.Mappers
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
    }
}
