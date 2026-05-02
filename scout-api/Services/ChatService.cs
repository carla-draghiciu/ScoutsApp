using MongoDB.Bson;
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

        public async Task<List<ChatMessage>> GetUserConversationsAsync(int userId)
        {
            // Match any roomId that contains this userId
            var filter = Builders<ChatMessage>.Filter.Regex(
                m => m.RoomId,
                new BsonRegularExpression($"chat_.*{userId}.*")
            );

            // Get the latest message per room
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("RoomId",
                    new BsonDocument("$regex", $"chat_.*{userId}.*"))),
                new BsonDocument("$sort", new BsonDocument("TimeStamp", -1)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$RoomId" },
                    { "lastMessage", new BsonDocument("$first", "$$ROOT") }
                }),
                new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$lastMessage"))
            };

            return await messages.Aggregate<ChatMessage>(pipeline).ToListAsync();
        }
    }
}
