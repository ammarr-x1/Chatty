using System.Threading.Tasks;

namespace Chatty.Services
{
    public class DalUser
    {
        public Task<bool> AuthenticateUser(string username, string password)
        {
            // Simple mock authentication: allow any non-empty username
            return Task.FromResult(!string.IsNullOrWhiteSpace(username));
        }
    }
}
