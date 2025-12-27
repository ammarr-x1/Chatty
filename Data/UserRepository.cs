using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatty.Models;
using MongoDB.Driver;

namespace Chatty.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;

        public UserRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.Find(u => u.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }
    }
}
