using Microsoft.AspNetCore.SignalR;
using scout_api.Models;
using scout_api.Repositories;

namespace scout_api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatRepository chatService;

        public ChatHub(ChatRepository chatService)
        {
            this.chatService = chatService;
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var chatHistory = await chatService.GetRoomHistoryAsync(roomId);
            await Clients.Caller.SendAsync("LoadChatHistory", chatHistory);
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task SendMessage(string roomId, int senderId, string senderName, string messageContent)
        {
            var messageToSend = new ChatMessage
            {
                RoomId = roomId,
                SenderId = senderId,
                SenderName = senderName,
                Content = messageContent,
                TimeStamp = DateTime.UtcNow
            };

            await chatService.SaveMessageAsync(messageToSend);

            // broadcast to everyone in the room
            await Clients.Group(roomId).SendAsync("ReceiveMessage", messageToSend);
        }
    }
}
