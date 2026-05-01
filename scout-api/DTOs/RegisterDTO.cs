using scout_api.Models;

namespace scout_api.DTOs
{
    public class RegisterDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ScoutId { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string ScoutLevel { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
