using Microsoft.EntityFrameworkCore;
using scout_api.DTOs;
using scout_api.Enums;
using scout_api.Mappers;
using scout_api.Models;
using scout_api.Validators;

namespace scout_api.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext databaseContext;
        private readonly SessionRepository sessionRepository;

        public UserRepository(AppDbContext databaseContext, SessionRepository sessionRepository)
        {
            this.databaseContext = databaseContext;
            this.sessionRepository = sessionRepository;
        }

        public int GetSeshCount()
        {
            return this.sessionRepository.Sessions.Count;
        }

        public List<User> GetAll()
        {
            return databaseContext.Users
                .Include(user => user.EarnedBadges)
                .ToList();
        }

        public Dictionary<string, User> GetAllLoggedIn()
        {
            return this.sessionRepository.Sessions;
        }

        public User? FindUserByEmail(string email)
        {
            return databaseContext.Users
                .FirstOrDefault(user => user.Email == email);
        }

        public User? GetUserByToken(string token)
        {
            this.sessionRepository.Sessions.TryGetValue(token, out User? user);
            return user;
        }

        public UserDTO? GetUserById(int userId)
        {
            return databaseContext.Users
                .Include(user => user.EarnedBadges)
                .FirstOrDefault(user => user.Id == userId)
                .ToDto();
        }

        public User? GetUserByIdentifier(string uniqueIdentifier)
        {
            return databaseContext.Users
                .Include(user => user.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefault(user => user.Email == uniqueIdentifier || user.ScoutId == uniqueIdentifier || user.Name == uniqueIdentifier);
        }

        public User AddUser(User user)
        {
            databaseContext.Users.Add(user);
            databaseContext.SaveChanges();
            return user;
        }

        public (User user, string token, string role, List<string> permissions)? Login(User user)
        {
            var token = Guid.NewGuid().ToString();
            this.sessionRepository.Sessions[token] = user;

            var permissions = user.Role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList();

            return (user, token, user.Role.Name, permissions);
        }

        public void Logout(string token)
        {
            this.sessionRepository.Sessions.Remove(token);
        }
    }
}