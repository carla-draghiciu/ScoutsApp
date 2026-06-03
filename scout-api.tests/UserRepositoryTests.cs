using Microsoft.EntityFrameworkCore;
using scout_api.Models;
using scout_api.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scout_api.tests
{
    [TestClass]
    public class UserRepositoryTests
    {
        private AppDbContext _dbContext = null!;
        private SessionRepository _sessionRepository = null!;
        private UserRepository _userRepository = null!;

        private static Permission MakePermission(string name) =>
            new() { Name = name };

        private static Role MakeRole(string name, IEnumerable<string>? permissions = null)
        {
            var role = new Role { Name = name, RolePermissions = new List<RolePermission>() };
            foreach (var perm in permissions ?? Enumerable.Empty<string>())
                role.RolePermissions.Add(new RolePermission { Permission = MakePermission(perm) });
            return role;
        }

        private static User MakeUser(
            string name = "Alice",
            string email = "alice@example.com",
            string scoutId = "SC001",
            Role? role = null) =>
            new()
            {
                Name = name,
                Email = email,
                ScoutId = scoutId,
                Role = role ?? MakeRole("Member", new[] { "read" })
            };

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
                .Options;

            _dbContext = new AppDbContext(options);
            _sessionRepository = new SessionRepository();
            _userRepository = new UserRepository(_dbContext, _sessionRepository);
        }

        [TestCleanup]
        public void Cleanup() => _dbContext.Dispose();

        [TestMethod]
        public async Task GetSeshCount_ReturnsZero_WhenNoSessions()
        {
            Assert.AreEqual(0, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public async Task GetSeshCount_ReflectsSessionCount_AfterLogins()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            _userRepository.Login(user);
            _userRepository.Login(user); // same user, different tokens each time

            Assert.AreEqual(2, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public async Task GetAll_ReturnsEmptyList_WhenNoUsers()
        {
            var result = await _userRepository.GetAllAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllLoggedIn_ReturnsEmptyDictionary_Initially()
        {
            var result = _userRepository.GetAllLoggedIn();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllLoggedIn_ContainsUser_AfterLogin()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var loginResult = _userRepository.Login(user);
            var sessions = _userRepository.GetAllLoggedIn();

            Assert.IsTrue(sessions.ContainsKey(loginResult.token));
        }

        [TestMethod]
        public async Task FindUserByEmail_ReturnsNull_WhenEmailNotFound()
        {
            var result = await _userRepository.FindUserByEmailAsync("ghost@example.com");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FindUserByEmail_ReturnsUser_WhenEmailExists()
        {
            var user = MakeUser(email: "found@example.com");
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = await _userRepository.FindUserByEmailAsync("found@example.com");

            Assert.IsNotNull(result);
            Assert.AreEqual("found@example.com", result.Email);
        }

        [TestMethod]
        public async Task GetUserByToken_ReturnsNull_ForUnknownToken()
        {
            var result = _userRepository.GetUserByToken("nonexistent-token");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetUserByToken_ReturnsUser_ForValidToken()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var loginResult = _userRepository.Login(user);
            var result = _userRepository.GetUserByToken(loginResult.token);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result.Email);
        }

        [TestMethod]
        public async Task GetUserById_ReturnsNull_WhenIdNotFound()
        {
            var result = await _userRepository.GetUserByIdAsync(999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetUserById_ReturnsDto_WhenIdExists()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = await _userRepository.GetUserByIdAsync(user.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result.Email);
        }

        [TestMethod]
        [DataRow("alice@example.com")]
        [DataRow("SC001")]
        [DataRow("Alice")]
        public async Task GetUserByIdentifier_FindsUser_ByEmailScoutIdOrName(string identifier)
        {
            var role = MakeRole("Leader", new[] { "read", "write" });
            var user = MakeUser(name: "Alice", email: "alice@example.com", scoutId: "SC001", role: role);
            _dbContext.Roles.Add(role);
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = await _userRepository.GetUserByIdentifierAsync(identifier);

            Assert.IsNotNull(result);
            Assert.AreEqual("alice@example.com", result.Email);
        }

        [TestMethod]
        public async Task GetUserByIdentifier_IncludesRoleAndPermissions()
        {
            var role = MakeRole("Admin", new[] { "read", "write", "delete" });
            var user = MakeUser(role: role);
            _dbContext.Roles.Add(role);
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = await _userRepository.GetUserByIdentifierAsync(user.Email);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Role);
            Assert.AreEqual(3, result.Role.RolePermissions.Count);
        }

        [TestMethod]
        public async Task GetUserByIdentifier_ReturnsNull_WhenNoMatch()
        {
            var result = await _userRepository.GetUserByIdentifierAsync("no-match");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AddUser_PersistsUserToDatabase()
        {
            var user = MakeUser();
            var result = await _userRepository.AddUserAsync(user);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, _dbContext.Users.Count());
        }

        [TestMethod]
        public async Task AddUser_AssignsId_AfterSave()
        {
            var user = MakeUser();
            await _userRepository.AddUserAsync(user);

            Assert.IsTrue(user.Id > 0);
        }

        [TestMethod]
        public async Task Login_ReturnsNonNullResult()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = _userRepository.Login(user);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Login_CreatesSession_WithGuidToken()
        {
            var user = MakeUser();
            var result = _userRepository.Login(user);

            Assert.IsTrue(Guid.TryParse(result.token, out _));
        }

        [TestMethod]
        public async Task Login_ReturnsCorrectRoleAndPermissions()
        {
            var role = MakeRole("Leader", new[] { "read", "write" });
            var user = MakeUser(role: role);

            var result = _userRepository.Login(user);

            Assert.AreEqual("Leader", result.role);
            CollectionAssert.AreEquivalent(new[] { "read", "write" }, result.permissions);
        }

        [TestMethod]
        public async Task Login_AddsTokenToSessions()
        {
            var user = MakeUser();
            var result = _userRepository.Login(user);

            Assert.IsTrue(_sessionRepository.Sessions.ContainsKey(result.token));
        }

        [TestMethod]
        public async Task Login_MultipleLogins_CreateDistinctTokens()
        {
            var user = MakeUser();
            var result1 = _userRepository.Login(user);
            var result2 = _userRepository.Login(user);

            Assert.AreNotEqual(result1.token, result2.token);
            Assert.AreEqual(2, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public async Task Logout_RemovesSession_ByToken()
        {
            var user = MakeUser();
            var result = _userRepository.Login(user);
            var token = result.token;

            _userRepository.Logout(token);

            Assert.IsFalse(_sessionRepository.Sessions.ContainsKey(token));
        }

        [TestMethod]
        public async Task Logout_WithUnknownToken_DoesNotThrow()
        {
            // Should not throw for a token that was never added
            _userRepository.Logout("made-up-token");

            Assert.AreEqual(0, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public async Task Logout_OnlyRemovesTargetSession_LeavingOthersIntact()
        {
            var user = MakeUser();
            var result1 = _userRepository.Login(user);
            var result2 = _userRepository.Login(user);

            _userRepository.Logout(result1.token);

            Assert.AreEqual(1, _userRepository.GetSeshCount());
            Assert.IsTrue(_sessionRepository.Sessions.ContainsKey(result2.token));
        }
    }
}
