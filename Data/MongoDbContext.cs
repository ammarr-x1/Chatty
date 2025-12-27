using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Chatty.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("MongoDB:ConnectionString");
            var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName");

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Chatty.Models.User> Users => _database.GetCollection<Chatty.Models.User>("Users");
    }
}
