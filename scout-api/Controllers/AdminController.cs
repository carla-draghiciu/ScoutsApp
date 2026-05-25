using Microsoft.AspNetCore.Mvc;
using scout_api.Repositories;
using scout_api.Services;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : BaseController
    {
        private readonly LoggingService _loggingService;
        private readonly SessionRepository _sessionService;

        public AdminController(LoggingService loggingService, SessionRepository sessionService, UserService userService) : base(userService)
        {
            _loggingService = loggingService;
            _sessionService = sessionService;
        }

        private bool IsAdmin(HttpContext context)
        {
            var currentUser = GetCurrentUser();
            return currentUser?.Role?.Name == "Admin";
        }

        [HttpGet("observation-list")]
        public async Task<IActionResult> GetObservationList()
        {
            if (!IsAdmin(HttpContext))
            {
                return Forbid();
            }

            var list = await _loggingService.GetObservationListAsync();
            return Ok(list);
        }

        [HttpGet("logs/{userId}")]
        public async Task<IActionResult> GetUserLogs(int userId)
        {
            if (!IsAdmin(HttpContext))
            {
                return Forbid();
            }

            var logs = await _loggingService.GetUserLogsAsync(userId);
            return Ok(logs);
        }

        [HttpPatch("observation-list/{entryId}/resolve")]
        public async Task<IActionResult> ResolveEntry(int entryId)
        {
            if (!IsAdmin(HttpContext))
            {
                return Forbid();
            }

            await _loggingService.ResolveEntryAsync(entryId);
            return Ok();
        }
    }
}
