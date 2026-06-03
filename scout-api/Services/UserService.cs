using scout_api.DTOs;
using scout_api.Enums;
using scout_api.Mappers;
using scout_api.Models;
using scout_api.Repositories;
using scout_api.Validators;

namespace scout_api.Services
{
    public class UserService
    {
        private readonly UserRepository userRepository;

        public UserService(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public Dictionary<string, User> GetAllLoggedIn()
        {
            return userRepository.GetAllLoggedIn();
        }

        public async Task<List<UserDTO?>> GetAllAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            return await userRepository.GetUserByIdAsync(userId);
        }

        public User? GetUserByToken(string token)
        {
            return userRepository.GetUserByToken(token);
        }

        public async Task<User?> RegisterAsync(RegisterDTO registeringUser)
        {
            User? user = await userRepository.FindUserByEmailAsync(registeringUser.Email);
            if (user != null)
            {
                return null;
            }

            if (!Enum.TryParse<ScoutLevel>(registeringUser.ScoutLevel, true, out var level))
            {
                return null; // invalid value sent for enum
            }

            ValidateRegisteringUser(registeringUser);

            User registered = registeringUser.FromDto(level);

            return await userRepository.AddUserAsync(registered);
        }

        public async Task<(User user, string token, string role, List<string> permissions)?> LoginAsync(LoginDTO logingUser)
        {
            User? user = await userRepository.GetUserByIdentifierAsync(logingUser.Identifier);
            
            if (user == null)
            {
                return null;
            }

            if (!PasswordMatches(user.Password, logingUser.Password))
            {
                return null;
            }

            return userRepository.Login(user);
        }

        public void Logout(string token)
        {
            userRepository.Logout(token);
        }

        private void ValidateRegisteringUser(RegisterDTO registeringUser)
        {
            UserValidator.ValidateName(registeringUser.Name);
            UserValidator.ValidateEmail(registeringUser.Email);
            UserValidator.ValidateDateOfBirth(DateTime.Parse(registeringUser.DateOfBirth));
            UserValidator.ValidatePassword(registeringUser.Password);
        }

        private bool PasswordMatches(string accountPassword, string enteredPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, accountPassword);
        }
    }
}
