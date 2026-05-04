using Microsoft.AspNetCore.Mvc;
using scout_api.Services;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly LoggingService _loggingService;
        private readonly SessionService _sessionService;

        public AdminController(LoggingService loggingService, SessionService sessionService)
        {
            _loggingService = loggingService;
            _sessionService = sessionService;
        }

        private bool IsAdmin(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = _sessionService.Sessions.GetValueOrDefault(token);
            return user?.Role?.Name == "Admin";
        }

        [HttpGet("observation-list")]
        public async Task<IActionResult> GetObservationList()
        {
            if (!IsAdmin(HttpContext)) return Forbid();
            var list = await _loggingService.GetObservationListAsync();
            return Ok(list);
        }

        [HttpGet("logs/{userId}")]
        public async Task<IActionResult> GetUserLogs(int userId)
        {
            if (!IsAdmin(HttpContext)) return Forbid();
            var logs = await _loggingService.GetUserLogsAsync(userId);
            return Ok(logs);
        }

        [HttpPatch("observation-list/{entryId}/resolve")]
        public async Task<IActionResult> ResolveEntry(int entryId)
        {
            if (!IsAdmin(HttpContext)) return Forbid();
            await _loggingService.ResolveEntryAsync(entryId);
            return Ok();
        }
    }
}
