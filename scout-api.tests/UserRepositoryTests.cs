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
        public void GetSeshCount_ReturnsZero_WhenNoSessions()
        {
            Assert.AreEqual(0, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public void GetSeshCount_ReflectsSessionCount_AfterLogins()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            _userRepository.Login(user);
            _userRepository.Login(user); // same user, different tokens each time

            Assert.AreEqual(2, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public void GetAll_ReturnsEmptyList_WhenNoUsers()
        {
            var result = _userRepository.GetAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetAllLoggedIn_ReturnsEmptyDictionary_Initially()
        {
            var result = _userRepository.GetAllLoggedIn();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetAllLoggedIn_ContainsUser_AfterLogin()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var loginResult = _userRepository.Login(user);
            var sessions = _userRepository.GetAllLoggedIn();

            Assert.IsTrue(sessions.ContainsKey(loginResult!.Value.token));
        }

        [TestMethod]
        public void FindUserByEmail_ReturnsNull_WhenEmailNotFound()
        {
            var result = _userRepository.FindUserByEmail("ghost@example.com");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindUserByEmail_ReturnsUser_WhenEmailExists()
        {
            var user = MakeUser(email: "found@example.com");
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = _userRepository.FindUserByEmail("found@example.com");

            Assert.IsNotNull(result);
            Assert.AreEqual("found@example.com", result.Email);
        }

        [TestMethod]
        public void GetUserByToken_ReturnsNull_ForUnknownToken()
        {
            var result = _userRepository.GetUserByToken("nonexistent-token");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetUserByToken_ReturnsUser_ForValidToken()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var loginResult = _userRepository.Login(user);
            var result = _userRepository.GetUserByToken(loginResult!.Value.token);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result.Email);
        }

        [TestMethod]
        public void GetUserById_ReturnsNull_WhenIdNotFound()
        {
            var result = _userRepository.GetUserById(999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetUserById_ReturnsDto_WhenIdExists()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = _userRepository.GetUserById(user.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result.Email);
        }

        [TestMethod]
        [DataRow("alice@example.com")]
        [DataRow("SC001")]
        [DataRow("Alice")]
        public void GetUserByIdentifier_FindsUser_ByEmailScoutIdOrName(string identifier)
        {
            var role = MakeRole("Leader", new[] { "read", "write" });
            var user = MakeUser(name: "Alice", email: "alice@example.com", scoutId: "SC001", role: role);
            _dbContext.Roles.Add(role);
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = _userRepository.GetUserByIdentifier(identifier);

            Assert.IsNotNull(result);
            Assert.AreEqual("alice@example.com", result.Email);
        }

        [TestMethod]
        public void GetUserByIdentifier_IncludesRoleAndPermissions()
        {
            var role = MakeRole("Admin", new[] { "read", "write", "delete" });
            var user = MakeUser(role: role);
            _dbContext.Roles.Add(role);
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = _userRepository.GetUserByIdentifier(user.Email);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Role);
            Assert.AreEqual(3, result.Role.RolePermissions.Count);
        }

        [TestMethod]
        public void GetUserByIdentifier_ReturnsNull_WhenNoMatch()
        {
            var result = _userRepository.GetUserByIdentifier("no-match");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddUser_PersistsUserToDatabase()
        {
            var user = MakeUser();
            var result = _userRepository.AddUser(user);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, _dbContext.Users.Count());
        }

        [TestMethod]
        public void AddUser_AssignsId_AfterSave()
        {
            var user = MakeUser();
            _userRepository.AddUser(user);

            Assert.IsTrue(user.Id > 0);
        }

        [TestMethod]
        public void Login_ReturnsNonNullResult()
        {
            var user = MakeUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var result = _userRepository.Login(user);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Login_CreatesSession_WithGuidToken()
        {
            var user = MakeUser();
            var result = _userRepository.Login(user);

            Assert.IsTrue(Guid.TryParse(result!.Value.token, out _));
        }

        [TestMethod]
        public void Login_ReturnsCorrectRoleAndPermissions()
        {
            var role = MakeRole("Leader", new[] { "read", "write" });
            var user = MakeUser(role: role);

            var result = _userRepository.Login(user);

            Assert.AreEqual("Leader", result!.Value.role);
            CollectionAssert.AreEquivalent(new[] { "read", "write" }, result.Value.permissions);
        }

        [TestMethod]
        public void Login_AddsTokenToSessions()
        {
            var user = MakeUser();
            var result = _userRepository.Login(user);

            Assert.IsTrue(_sessionRepository.Sessions.ContainsKey(result!.Value.token));
        }

        [TestMethod]
        public void Login_MultipleLogins_CreateDistinctTokens()
        {
            var user = MakeUser();
            var result1 = _userRepository.Login(user);
            var result2 = _userRepository.Login(user);

            Assert.AreNotEqual(result1!.Value.token, result2!.Value.token);
            Assert.AreEqual(2, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public void Logout_RemovesSession_ByToken()
        {
            var user = MakeUser();
            var result = _userRepository.Login(user);
            var token = result!.Value.token;

            _userRepository.Logout(token);

            Assert.IsFalse(_sessionRepository.Sessions.ContainsKey(token));
        }

        [TestMethod]
        public void Logout_WithUnknownToken_DoesNotThrow()
        {
            // Should not throw for a token that was never added
            _userRepository.Logout("made-up-token");

            Assert.AreEqual(0, _userRepository.GetSeshCount());
        }

        [TestMethod]
        public void Logout_OnlyRemovesTargetSession_LeavingOthersIntact()
        {
            var user = MakeUser();
            var result1 = _userRepository.Login(user);
            var result2 = _userRepository.Login(user);

            _userRepository.Logout(result1!.Value.token);

            Assert.AreEqual(1, _userRepository.GetSeshCount());
            Assert.IsTrue(_sessionRepository.Sessions.ContainsKey(result2!.Value.token));
        }
    }
}
