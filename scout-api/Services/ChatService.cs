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
    }
}
