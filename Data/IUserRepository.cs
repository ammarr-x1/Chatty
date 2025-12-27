using System.Threading.Tasks;
using Chatty.Models;

namespace Chatty.Data
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task CreateUserAsync(User user);
    }
}
