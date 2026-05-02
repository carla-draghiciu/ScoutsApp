using MongoDB.Driver;
using scout_api.Models;

namespace scout_api.Services
{
    public class ChatService
    {
        private readonly IMongoCollection<ChatMessage> messages;

        public ChatService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
            messages = database.GetCollection<ChatMessage>("messages");
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            await messages.InsertOneAsync(message);
        }

        public async Task<List<ChatMessage>> GetRoomHistoryAsync(string roomId, int limit = 50)
        {
            return await messages
                .Find(message => message.RoomId == roomId)
                .SortByDescending(message => message.TimeStamp)
                .Limit(limit)
                .ToListAsync();
        }
    }
}
