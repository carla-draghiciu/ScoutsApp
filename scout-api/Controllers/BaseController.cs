using Microsoft.AspNetCore.Mvc;
using scout_api.Models;
using scout_api.Repositories;

namespace scout_api.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly UserRepository userService;

        public BaseController(UserRepository userService)
        {
            this.userService = userService;
        }

        protected User? GetCurrentUser()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return userService.GetUserByToken(token);
        }

        protected bool HasPermission(User user, string permission)
        {
            return user.Role.RolePermissions.Any(rp => rp.Permission.Name == permission);
        }

        protected IActionResult? CheckPermission(string permission)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!HasPermission(currentUser, permission))
            {
                return Forbid();
            }

            return null;
        }
    }
}
