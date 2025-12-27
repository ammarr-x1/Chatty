using System;

namespace Chatty.Models
{
    public class User
    {
        // MongoDB _id usually maps to string or ObjectId. 
        // Using string for simplicity in placeholder, but typically looks like:
        // [BsonId]
        // [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Username { get; set; }
        
        public string PasswordHash { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
