using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using scout_api.DTOs;
using scout_api.Repositories;
using scout_api.Services;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthentificatorController : Controller
    {
        private readonly UserService userService;

        public AuthentificatorController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await userService.GetAllAsync());
        }

        [HttpGet("/current")]
        public IActionResult GetAllCurrent()
        {
            return Ok(userService.GetAllLoggedIn());
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(int userId)
        {
            var foundUser = await userService.GetUserByIdAsync(userId);

            if (foundUser == null)
            {
                return NotFound();
            }

            return Ok(foundUser);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registeringUser)
        {
            try
            {
                var user = await userService.RegisterAsync(registeringUser);
                if (user == null)
                {
                    return Conflict("An account already exists with this email");
                }

                return Ok(new
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO logingUser)
        {
            var loggedIn = await userService.LoginAsync(logingUser);
            if (loggedIn == null)
            {
                return Unauthorized("Incorrect email or password");
            }
            var (user, token, role, permissions) = loggedIn.Value;

            return Ok(new
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                Role = role,
                Permissions = permissions
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            userService.Logout(token);

            return Ok();
        }
    }
}
