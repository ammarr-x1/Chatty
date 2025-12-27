using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Chatty.Data;
using Chatty.Models;

namespace Chatty.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            var existing = await _userRepository.GetByUsernameAsync(username);
            if (existing != null)
                return null; // Username taken

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password)
            };

            await _userRepository.CreateUserAsync(user);
            return user;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return null;

            var inputHash = HashPassword(password);
            if (user.PasswordHash != inputHash)
                return null;

            return user;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
