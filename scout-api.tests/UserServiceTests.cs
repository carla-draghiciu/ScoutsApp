using Microsoft.EntityFrameworkCore;
using scout_api.DTOs;
using scout_api.Enums;
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
    public sealed class UserServiceTests
    {

        private AppDbContext _context;
        private UserRepository _userService;
        private SessionRepository _sessionService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sessionService = new SessionRepository();
            _userService = new UserRepository(_context, _sessionService);

            var adminRole = new Role { Id = 1, Name = "Admin" };
            var userRole = new Role { Id = 2, Name = "User" };
            _context.Roles.AddRange(adminRole, userRole);

            var permissions = new List<Permission>
            {
                new Permission { Id = 1, Name = "manage_badges" },
                new Permission { Id = 2, Name = "create_event" },
                new Permission { Id = 3, Name = "update_event" },
                new Permission { Id = 4, Name = "delete_event" },
                new Permission { Id = 5, Name = "view_events" },
                new Permission { Id = 6, Name = "join_event" }
            };
            _context.Permissions.AddRange(permissions);

            _context.RolePermissions.AddRange(
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 4 },
                new RolePermission { RoleId = 1, PermissionId = 5 },
                new RolePermission { RoleId = 1, PermissionId = 6 },
                new RolePermission { RoleId = 2, PermissionId = 2 },
                new RolePermission { RoleId = 2, PermissionId = 3 },
                new RolePermission { RoleId = 2, PermissionId = 4 },
                new RolePermission { RoleId = 2, PermissionId = 5 },
                new RolePermission { RoleId = 2, PermissionId = 6 }
            );

            _context.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private User SeedUser(string email = "test@test.com", string name = "Test User", int roleId = 2)
        {
            var user = new User
            {
                Name = name,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                ScoutId = "TST001",
                DateOfBirth = new DateTime(1995, 1, 1),
                ScoutLevel = ScoutLevel.Explorator,
                RoleId = roleId
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        private RegisterDTO ValidRegisterDto(string email = "new@test.com") => new RegisterDTO
        {
            Name = "New User",
            Email = email,
            Password = "Password123!",
            ScoutId = "NEW001",
            DateOfBirth = "1995-06-15",
            ScoutLevel = "Junior"
        };

        private LoginDTO LoginDtoFor(string email, string password = "Password123!") => new LoginDTO
        {
            Identifier = email,
            Password = password
        };


        [TestMethod]
        public void GetAll_ReturnsAllUsers()
        {
            SeedUser("user1@test.com");
            SeedUser("user2@test.com");

            var result = _userService.GetAll();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetAll_EmptyDatabase_ReturnsEmptyList()
        {
            var result = _userService.GetAll();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindUserByEmail_ExistingEmail_ReturnsUser()
        {
            SeedUser("find@test.com");

            var result = _userService.FindUserByEmail("find@test.com");

            Assert.IsNotNull(result);
            Assert.AreEqual("find@test.com", result.Email);
        }

        [TestMethod]
        public void FindUserByEmail_NonExistingEmail_ReturnsNull()
        {
            var result = _userService.FindUserByEmail("ghost@test.com");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Register_DuplicateEmail_ReturnsNull()
        {
            SeedUser("existing@test.com");
            var dto = ValidRegisterDto("existing@test.com");

            var result = _userService.Register(dto);

            Assert.IsNull(result);
            Assert.AreEqual(1, _context.Users.Count()); // no new user added
        }

        [TestMethod]
        public void Register_InvalidScoutLevel_ReturnsNull()
        {
            var dto = ValidRegisterDto();
            dto.ScoutLevel = "InvalidLevel";

            var result = _userService.Register(dto);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Login_ValidCredentials_ReturnsUserAndToken()
        {
            SeedUser("login@test.com");
            var dto = LoginDtoFor("login@test.com");

            var result = _userService.Login(dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("login@test.com", result.Value.user.Email);
            Assert.IsFalse(string.IsNullOrEmpty(result.Value.token));
        }

        [TestMethod]
        public void Login_ValidCredentials_AddsToSession()
        {
            SeedUser("session@test.com");
            var dto = LoginDtoFor("session@test.com");

            var result = _userService.Login(dto);

            Assert.IsTrue(_sessionService.Sessions.ContainsKey(result.Value.token));
        }

        [TestMethod]
        public void Login_ValidCredentials_ReturnsRoleAndPermissions()
        {
            SeedUser("role@test.com", roleId: 2);
            var dto = LoginDtoFor("role@test.com");

            var result = _userService.Login(dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("User", result.Value.role);
            Assert.IsTrue(result.Value.permissions.Count > 0);
            CollectionAssert.Contains(result.Value.permissions, "view_events");
        }

        [TestMethod]
        public void Login_AdminUser_ReturnsAllPermissions()
        {
            SeedUser("admin@test.com", roleId: 1);
            var dto = LoginDtoFor("admin@test.com");

            var result = _userService.Login(dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("Admin", result.Value.role);
            Assert.AreEqual(6, result.Value.permissions.Count);
            CollectionAssert.Contains(result.Value.permissions, "manage_badges");
        }

        [TestMethod]
        public void Login_WrongPassword_ReturnsNull()
        {
            SeedUser("wrong@test.com");
            var dto = LoginDtoFor("wrong@test.com", "WrongPassword!");

            var result = _userService.Login(dto);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Login_NonExistingEmail_ReturnsNull()
        {
            var dto = LoginDtoFor("ghost@test.com");

            var result = _userService.Login(dto);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Logout_ValidToken_RemovesFromSession()
        {
            SeedUser("logout@test.com");
            var loginResult = _userService.Login(LoginDtoFor("logout@test.com"));
            var token = loginResult!.Value.token;

            _userService.Logout(token);

            Assert.IsFalse(_sessionService.Sessions.ContainsKey(token));
        }

        [TestMethod]
        public void Logout_InvalidToken_DoesNotThrow()
        {
            _userService.Logout("non-existent-token");
        }

        [TestMethod]
        public void GetUserByToken_ValidToken_ReturnsUser()
        {
            SeedUser("token@test.com");
            var loginResult = _userService.Login(LoginDtoFor("token@test.com"));
            var token = loginResult!.Value.token;

            var result = _userService.GetUserByToken(token);

            Assert.IsNotNull(result);
            Assert.AreEqual("token@test.com", result.Email);
        }

        [TestMethod]
        public void GetUserByToken_InvalidToken_ReturnsNull()
        {
            var result = _userService.GetUserByToken("fake-token");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetUserById_ExistingId_ReturnsUserDto()
        {
            var seeded = SeedUser("byid@test.com");

            var result = _userService.GetUserById(seeded.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("byid@test.com", result.Email);
        }

        [TestMethod]
        public void GetUserById_NonExistingId_ReturnsNull()
        {
            var result = _userService.GetUserById(9999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetSeshCount_NoSessions_ReturnsZero()
        {
            var result = _userService.GetSeshCount();

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetSeshCount_AfterLogin_ReturnsCorrectCount()
        {
            SeedUser("count1@test.com");
            SeedUser("count2@test.com");

            _userService.Login(LoginDtoFor("count1@test.com"));
            _userService.Login(LoginDtoFor("count2@test.com"));

            Assert.AreEqual(2, _userService.GetSeshCount());
        }

        [TestMethod]
        public void GetAllLoggedIn_ReturnsCurrentSessions()
        {
            SeedUser("logged@test.com");
            _userService.Login(LoginDtoFor("logged@test.com"));

            var result = _userService.GetAllLoggedIn();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Values.Any(u => u.Email == "logged@test.com"));
        }
    }
}