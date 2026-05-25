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

        public List<User> GetAll()
        {
            return userRepository.GetAll();
        }

        public UserDTO? GetUserById(int userId)
        {
            return userRepository.GetUserById(userId);
        }

        public User? GetUserByToken(string token)
        {
            return userRepository.GetUserByToken(token);
        }

        public User? Register(RegisterDTO registeringUser)
        {
            User? user = userRepository.FindUserByEmail(registeringUser.Email);
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

            return userRepository.AddUser(registered);
        }

        public (User user, string token, string role, List<string> permissions)? Login(LoginDTO logingUser)
        {
            User? user = userRepository.GetUserByIdentifier(logingUser.Identifier);
            
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
