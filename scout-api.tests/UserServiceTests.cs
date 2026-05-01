//using scout_api.DTOs;
//using scout_api.Services;
//using scout_api.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace scout_api.tests
//{
//    [TestClass]
//    public sealed class UserServiceTests
//    {
//        private UserService _service;

//        [TestInitialize]
//        public void Setup()
//        {
//            _service = new UserService();
//        }

//        private RegisterDTO CreateValidRegisterDTO(string email = "test@test.com")
//        {
//            return new RegisterDTO
//            {
//                Name = "Test User",
//                Email = email,
//                ScoutId = "SC123",
//                DateOfBirth = "2000/01/01",
//                ScoutLevel = "Senior",
//                Password = "Password123"
//            };
//        }

//        private LoginDTO CreateValidLoginDTO(string email, string password)
//        {
//            return new LoginDTO
//            {
//                Email = email,
//                Password = password
//            };
//        }

//        [TestMethod]
//        public void Register_ValidUser_ReturnsUser()
//        {
//            var dto = CreateValidRegisterDTO();

//            var result = _service.Register(dto);

//            Assert.IsNotNull(result);
//            Assert.AreEqual(dto.Email, result.Email);
//        }

//        [TestMethod]
//        public void Register_DuplicateEmail_ReturnsNull()
//        {
//            var dto = CreateValidRegisterDTO("dup@test.com");

//            _service.Register(dto);
//            var result = _service.Register(dto);

//            Assert.IsNull(result);
//        }

//        [TestMethod]
//        public void Register_InvalidScoutLevel_ReturnsNull()
//        {
//            var dto = CreateValidRegisterDTO();
//            dto.ScoutLevel = "INVALID";

//            var result = _service.Register(dto);

//            Assert.IsNull(result);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(Exception))]
//        public void Register_InvalidData_Throws()
//        {
//            var dto = new RegisterDTO
//            {
//                Name = "", // invalid
//                Email = "bad",
//                ScoutId = "x",
//                DateOfBirth = "2000/01/01",
//                ScoutLevel = "Senior",
//                Password = "123"
//            };

//            _service.Register(dto);
//        }

//        [TestMethod]
//        public void Login_ValidCredentials_ReturnsToken()
//        {
//            var dto = CreateValidRegisterDTO("login@test.com");
//            var user = _service.Register(dto);

//            var login = CreateValidLoginDTO(dto.Email, dto.Password);

//            var result = _service.Login(login);

//            Assert.IsNotNull(result);
//            Assert.AreEqual(user.Id, result.Value.user.Id);
//            Assert.IsFalse(string.IsNullOrEmpty(result.Value.token));
//        }

//        [TestMethod]
//        public void Login_UserNotFound_ReturnsNull()
//        {
//            var login = CreateValidLoginDTO("nouser@test.com", "Password123");

//            var result = _service.Login(login);

//            Assert.IsNull(result);
//        }

//        [TestMethod]
//        public void Login_WrongPassword_ReturnsNull()
//        {
//            var dto = CreateValidRegisterDTO("wrongpass@test.com");
//            _service.Register(dto);

//            var login = CreateValidLoginDTO(dto.Email, "WrongPassword");

//            var result = _service.Login(login);

//            Assert.IsNull(result);
//        }

//        [TestMethod]
//        public void Logout_RemovesSession()
//        {
//            var dto = CreateValidRegisterDTO("logout@test.com");
//            _service.Register(dto);

//            var login = _service.Login(CreateValidLoginDTO(dto.Email, dto.Password));

//            _service.Logout(login.Value.token);

//            Assert.AreEqual(0, _service.GetSeshCount());
//        }

//        [TestMethod]
//        public void GetUserByToken_ReturnsUser()
//        {
//            var dto = CreateValidRegisterDTO("token@test.com");
//            var user = _service.Register(dto);

//            var login = _service.Login(CreateValidLoginDTO(dto.Email, dto.Password));

//            var result = _service.GetUserByToken(login.Value.token);

//            Assert.IsNotNull(result);
//            Assert.AreEqual(user.Id, result.Id);
//        }

//        [TestMethod]
//        public void GetUserByToken_InvalidToken_ReturnsNull()
//        {
//            var result = _service.GetUserByToken("invalid");

//            Assert.IsNull(result);
//        }

//        [TestMethod]
//        public void FindUserByEmail_FindsUser()
//        {
//            var dto = CreateValidRegisterDTO("find@test.com");
//            var user = _service.Register(dto);

//            var result = _service.FindUserByEmail(dto.Email);

//            Assert.IsNotNull(result);
//        }

//        [TestMethod]
//        public void FindUserByEmail_NotFound_ReturnsNull()
//        {
//            var result = _service.FindUserByEmail("none@test.com");

//            Assert.IsNull(result);
//        }

//        [TestMethod]
//        public void GetUserById_FindsUser()
//        {
//            var dto = CreateValidRegisterDTO("id@test.com");
//            var user = _service.Register(dto);

//            var result = _service.GetUserById(user.Id);

//            Assert.IsNotNull(result);
//        }

//        [TestMethod]
//        public void GetUserById_NotFound_ReturnsNull()
//        {
//            var result = _service.GetUserById(999);

//            Assert.IsNull(result);
//        }

//        [TestMethod]
//        public void GetAll_ReturnsUsers()
//        {
//            var users = _service.GetAll();

//            Assert.IsTrue(users.Count > 0);
//        }

//        [TestMethod]
//        public void GetAllLoggedIn_ReturnsSessions()
//        {
//            var dto = CreateValidRegisterDTO("session@test.com");
//            _service.Register(dto);
//            _service.Login(CreateValidLoginDTO(dto.Email, dto.Password));

//            var sessions = _service.GetAllLoggedIn();

//            Assert.IsTrue(sessions.Count > 0);
//        }

//    }
//}
