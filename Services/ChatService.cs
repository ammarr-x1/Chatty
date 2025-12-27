using System;
using System.Collections.Generic;
using System.Linq;

namespace Chatty.Services
{
    public class ChatService
    {
        // Room Code -> List of Users
        private Dictionary<string, List<string>> _rooms = new Dictionary<string, List<string>>();
        
        // User -> List of Room Codes they are part of
        private Dictionary<string, List<string>> _userRooms = new Dictionary<string, List<string>>();

        public string CreateRoom(string creator)
        {
            string code = GenerateRoomCode();
            _rooms[code] = new List<string> { creator };
            
            AddUserToRoomList(creator, code);
            return code;
        }

        public string StartDirectChat(string user1, string user2)
        {
            // Create a deterministic unique ID for the pair (e.g., "Alice_Bob")
            var participants = new List<string> { user1, user2 };
            participants.Sort();
            string roomCode = $"DM-{string.Join("_", participants)}";

            if (!_rooms.ContainsKey(roomCode))
            {
                _rooms[roomCode] = new List<string> { user1, user2 };
                AddUserToRoomList(user1, roomCode);
                AddUserToRoomList(user2, roomCode);
            }
            
            return roomCode;
        }

        public bool JoinRoom(string username, string code)
        {
            if (!_rooms.ContainsKey(code)) return false;

            if (!_rooms[code].Contains(username))
            {
                _rooms[code].Add(username);
                AddUserToRoomList(username, code);
            }
            return true;
        }

        public List<string> GetUserChats(string username)
        {
            if (_userRooms.TryGetValue(username, out var rooms))
            {
                return rooms;
            }
            return new List<string>();
        }

        private void AddUserToRoomList(string username, string roomCode)
        {
            if (!_userRooms.ContainsKey(username))
            {
                _userRooms[username] = new List<string>();
            }
            if (!_userRooms[username].Contains(roomCode))
            {
                _userRooms[username].Add(roomCode);
            }
        }

        private string GenerateRoomCode()
        {
            return Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
        }
    }
}
