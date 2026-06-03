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

        public async Task<List<UserDTO?>> GetAllAsync()
        {
            return await databaseContext.Users
                .Include(user => user.EarnedBadges)
                .Select(user => user.ToDto())
                .ToListAsync();
        }

        public Dictionary<string, User> GetAllLoggedIn()
        {
            return this.sessionRepository.Sessions;
        }

        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await databaseContext.Users
                .FirstOrDefaultAsync(user => user.Email == email);
        }

        public User? GetUserByToken(string token)
        {
            this.sessionRepository.Sessions.TryGetValue(token, out User? user);
            return user;
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var user = await databaseContext.Users
                .Include(user => user.EarnedBadges)
                .FirstOrDefaultAsync(user => user.Id == userId);

            return user.ToDto();
        }

        public async Task<User?> GetUserByIdentifierAsync(string uniqueIdentifier)
        {
            return await databaseContext.Users
                .Include(user => user.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(user => user.Email == uniqueIdentifier || user.ScoutId == uniqueIdentifier || user.Name == uniqueIdentifier);
        }

        public async Task<User> AddUserAsync(User user)
        {
            databaseContext.Users.Add(user);
            await databaseContext.SaveChangesAsync();
            return user;
        }

        public (User user, string token, string role, List<string> permissions) Login(User user)
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
