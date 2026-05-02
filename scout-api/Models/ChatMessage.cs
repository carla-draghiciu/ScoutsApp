using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace scout_api.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RoomId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
