using Microsoft.EntityFrameworkCore;
using scout_api.DTOs;
using scout_api.Enums;
using scout_api.Mappers;
using scout_api.Models;
using scout_api.Validators;

namespace scout_api.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly SessionService sessionService;

        public UserService(AppDbContext context, SessionService sessionService)
        {
            _context = context;
            this.sessionService = sessionService;
        }

        public int GetSeshCount()
        {
            return this.sessionService.Sessions.Count;
        }

        public List<User> GetAll()
        {
            return _context.Users
                .Include(u => u.EarnedBadges)
                .ToList();
        }

        public Dictionary<string, User> GetAllLoggedIn()
        {
            return this.sessionService.Sessions;
        }

        public User? FindUserByEmail(string email)
        {
            return _context.Users
                .FirstOrDefault(u => u.Email == email);
        }

        private void ValidateRegisteringUser(RegisterDTO registeringUser)
        {
            UserValidator.ValidateName(registeringUser.Name);
            UserValidator.ValidateEmail(registeringUser.Email);
            UserValidator.ValidateDateOfBirth(DateTime.Parse(registeringUser.DateOfBirth));
            UserValidator.ValidatePassword(registeringUser.Password);
        }

        public User? Register(RegisterDTO registeringUser)
        {
            User? user = FindUserByEmail(registeringUser.Email);
            if (user != null)
            {
                return null;
            }

            if (!Enum.TryParse<ScoutLevel>(registeringUser.ScoutLevel, true, out var level))
                return null; // invalid value sent

            ValidateRegisteringUser(registeringUser);

            User registered = new User
            {
                //Id = nextAvailableId++,
                Name = registeringUser.Name,
                Email = registeringUser.Email,
                ScoutId = registeringUser.ScoutId,
                DateOfBirth = DateTime.Parse(registeringUser.DateOfBirth),
                ScoutLevel = level,
                Password = BCrypt.Net.BCrypt.HashPassword(registeringUser.Password),
                RoleId = 2
            };

            _context.Users.Add(registered);
            _context.SaveChanges();
            return registered;
        }

        private bool PasswordMatches(string accountPassword, string enteredPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, accountPassword);
        }

        public (User user, string token, string role, List<string> permissions)? Login(LoginDTO logingUser)
        {
            User? user = _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefault(u => u.Email == logingUser.Email);
            if (user == null)
            {
                return null;
            }

            if (!PasswordMatches(user.Password, logingUser.Password))
            {
                return null;
            }

            var token = Guid.NewGuid().ToString();
            this.sessionService.Sessions[token] = user;

            var permissions = user.Role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList();

            return (user, token, user.Role.Name, permissions);
        }

        public void Logout(string token)
        {
            this.sessionService.Sessions.Remove(token);
        }

        public User? GetUserByToken(string token)
        {
            this.sessionService.Sessions.TryGetValue(token, out User? user);
            return user;
        }

        public UserDTO? GetUserById(int id)
        {
            return _context.Users
                .Include(u => u.EarnedBadges)
                .FirstOrDefault(u => u.Id == id)
                .ToDto();
        }
    }
}