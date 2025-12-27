using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chatty.Services
{
    public class DalUser
    {
        // In-memory storage: Username -> Password
        private Dictionary<string, string> _users = new Dictionary<string, string>();

        public DalUser()
        {
            // Add some default demo users
            _users["admin"] = "admin123";
            _users["John"] = "john123";
            _users["Alice"] = "alice123";
        }

        public Task<bool> AuthenticateUser(string username, string password)
        {
            // Simple mock authentication: allow any non-empty username
            return Task.FromResult(!string.IsNullOrWhiteSpace(username));
        }
    }
}
