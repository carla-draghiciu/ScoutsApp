using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using scout_api.DTOs;
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
        public IActionResult GetAll()
        {
            return Ok(userService.GetAll());
        }

        [HttpGet("/current")]
        public IActionResult GetAllCurrent()
        {
            return Ok(userService.GetAllLoggedIn());
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDTO registeringUser)
        {
            try
            {
                var user = userService.Register(registeringUser);
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
        public IActionResult Login(LoginDTO logingUser)
        {
            var loggedIn = userService.Login(logingUser);
            if (loggedIn == null)
            {
                return Unauthorized("Incorrect email or password");
            }
            var (user, token) = loggedIn.Value;

            return Ok(new
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token
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
