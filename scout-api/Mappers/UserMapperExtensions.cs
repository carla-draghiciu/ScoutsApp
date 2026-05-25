using scout_api.DTOs;
using scout_api.Enums;
using scout_api.Models;

namespace scout_api.Mappers
{
    public static class UserMapperExtensions
    {
        public static UserDTO? ToDto(this User? scout)
        {
            if (scout == null)
                return null;
            return new UserDTO
            {
                Id = scout.Id,
                Name = scout.Name,
                Email = scout.Email,
                DateOfBirth = scout.DateOfBirth,
                ScoutLevel = scout.ScoutLevel.ToString()
            };
        }

        public static User FromDto(this RegisterDTO registeringUser, ScoutLevel level)
        {
            return new User
            {
                Name = registeringUser.Name,
                Email = registeringUser.Email,
                ScoutId = registeringUser.ScoutId,
                DateOfBirth = DateTime.Parse(registeringUser.DateOfBirth),
                ScoutLevel = level,
                Password = BCrypt.Net.BCrypt.HashPassword(registeringUser.Password),
                RoleId = 2
            };
        }
    }
}
