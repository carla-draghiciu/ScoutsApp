using Microsoft.AspNetCore.Mvc;
using scout_api.Repositories;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly ChatRepository chatService;

        public ChatController(ChatRepository chatService)
        {
            this.chatService = chatService;
        }

        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversations(int userId)
        {
            var conversations = await chatService.GetUserConversationsAsync(userId);
            return Ok(conversations);
        }
    }
}
