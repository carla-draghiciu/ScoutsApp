using scout_api.DTOs;
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
    }
}
