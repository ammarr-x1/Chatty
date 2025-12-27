using System;
using System.Collections.Generic;
using System.Linq;

namespace Chatty.Services
{
    public class ChatService
    {
        // Lock object for thread safety
        private readonly object _lock = new object();

        // Room Code -> List of Users
        private Dictionary<string, List<string>> _rooms = new Dictionary<string, List<string>>();
        
        // Room Code -> List of Messages
        private Dictionary<string, List<ChatMessage>> _roomMessages = new Dictionary<string, List<ChatMessage>>();

        // Event for real-time updates: RoomCode, MessageObject
        public event Action<string, ChatMessage>? OnMessageAdded;

        // User -> List of Room Codes they are part of
        private Dictionary<string, List<string>> _userRooms = new Dictionary<string, List<string>>();

        public string CreateRoom(string creator)
        {
            lock (_lock)
            {
                string code = GenerateRoomCode();
                _rooms[code] = new List<string> { creator };
                
                AddUserToRoomList(creator, code);
                return code;
            }
        }

        public string StartDirectChat(string user1, string user2)
        {
            // Create a deterministic unique ID for the pair (e.g., "Alice_Bob")
            var participants = new List<string> { user1, user2 };
            participants.Sort();
            string roomCode = $"DM-{string.Join("_", participants)}";

            lock (_lock)
            {
                if (!_rooms.ContainsKey(roomCode))
                {
                    _rooms[roomCode] = new List<string> { user1, user2 };
                    AddUserToRoomList(user1, roomCode);
                    AddUserToRoomList(user2, roomCode);
                }
            }
            
            return roomCode;
        }

        public bool JoinRoom(string username, string code)
        {
            lock (_lock)
            {
                if (!_rooms.ContainsKey(code)) return false;

                if (!_rooms[code].Contains(username))
                {
                    _rooms[code].Add(username);
                    AddUserToRoomList(username, code);
                }
                return true;
            }
        }

        public List<string> GetUserChats(string username)
        {
            lock (_lock)
            {
                if (_userRooms.TryGetValue(username, out var rooms))
                {
                    // Return a copy to avoid enumeration modification errors outside the lock
                    return new List<string>(rooms);
                }
                return new List<string>();
            }
        }

        public void DeleteChat(string username, string roomCode)
        {
            lock (_lock)
            {
                if (_userRooms.ContainsKey(username))
                {
                    _userRooms[username].Remove(roomCode);
                }
            }
        }

        public void AddMessage(string roomCode, string user, string message)
        {
            if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(message)) return;

            lock (_lock)
            {
                if (!_roomMessages.ContainsKey(roomCode))
                {
                    _roomMessages[roomCode] = new List<ChatMessage>();
                }
                
                _roomMessages[roomCode].Add(new ChatMessage 
                { 
                    User = user, 
                    Message = message, 
                    Timestamp = DateTime.Now 
                });

                // Keep only last 50 messages for memory safety in this demo
                if (_roomMessages[roomCode].Count > 50)
                {
                    _roomMessages[roomCode].RemoveAt(0);
                }
            }

            // Trigger event OUTSIDE the lock
            // We need to reconstruct the message object or grab it from the list? 
            // Better to create a new instance or pass the one we added.
            // Since we're outside lock, let's create a transient one for the event call
            var msgObj = new ChatMessage 
            { 
                User = user, 
                Message = message, 
                Timestamp = DateTime.Now // This might differ slightly from the one in list, but OK for this level
            };

            OnMessageAdded?.Invoke(roomCode, msgObj);
        }

        public List<ChatMessage> GetMessages(string roomCode)
        {
            lock (_lock)
            {
                if (_roomMessages.ContainsKey(roomCode))
                {
                    // Return a copy
                    return new List<ChatMessage>(_roomMessages[roomCode]);
                }
                return new List<ChatMessage>();
            }
        }

        private void AddUserToRoomList(string username, string roomCode)
        {
            // This is a private helper, assumed to be called within a lock already.
            // But to be safe, since it accesses _userRooms, we ensure the caller has the lock.
            // Or we just access the dict directly here since we know callers lock.
            
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

    public class ChatMessage
    {
        public string User { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
