// Services/GameManager.cs

using System.Collections.Concurrent;
using PacmanMultiplayer.Models;

namespace PacmanMultiplayer.Services;

public class GameManager
{
    // Thread-safe dictionary for managing multiple game rooms
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly object _lock = new object();

    public GameRoom? CreateOrJoinRoom(string roomCode, string connectionId, string username)
    {
        lock (_lock)
        {
            var room = _rooms.GetOrAdd(roomCode, code => new GameRoom(code));

            // Check if room is full
            if (room.GameState.Players.Count >= 5)
            {
                return null;
            }

            // Check if game already started
            if (room.GameState.Status == GameStatus.InProgress)
            {
                return null;
            }

            // Add player to room
            var player = new PlayerState
            {
                ConnectionId = connectionId,
                Username = username,
                IsAlive = true
            };

            room.GameState.Players[connectionId] = player;

            // First player is host
            if (room.GameState.Players.Count == 1)
            {
                room.GameState.HostConnectionId = connectionId;
            }

            return room;
        }
    }

    public bool RemovePlayer(string connectionId)
    {
        lock (_lock)
        {
            foreach (var room in _rooms.Values)
            {
                if (room.GameState.Players.TryGetValue(connectionId, out var player))
                {
                    // If game is in progress, we don't want to remove the player immediately
                    // as they might just be transitioning between pages (Lobby -> Game)
                    if (room.GameState.Status == GameStatus.InProgress)
                    {
                        return false;
                    }

                    room.GameState.Players.Remove(connectionId);

                    // If room is empty, remove it
                    if (room.GameState.Players.Count == 0)
                    {
                        _rooms.TryRemove(room.RoomCode, out _);
                    }
                    // If host left, assign new host
                    else if (room.GameState.HostConnectionId == connectionId)
                    {
                        room.GameState.HostConnectionId = room.GameState.Players.Keys.First();
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public GameState? ReconnectPlayer(string roomCode, string username, string newConnectionId)
    {
        lock (_lock)
        {
            var room = GetRoom(roomCode);
            if (room == null) return null;

            // Find player by username
            var playerEntry = room.GameState.Players.FirstOrDefault(p => p.Value.Username == username);
            if (playerEntry.Value == null) return null;

            // Remove old connection entry if it's different
            var oldConnectionId = playerEntry.Key;
            if (oldConnectionId != newConnectionId)
            {
                var playerState = playerEntry.Value;
                playerState.ConnectionId = newConnectionId;
                
                room.GameState.Players.Remove(oldConnectionId);
                room.GameState.Players[newConnectionId] = playerState;

                // Update host ID if necessary
                if (room.GameState.HostConnectionId == oldConnectionId)
                {
                    room.GameState.HostConnectionId = newConnectionId;
                }
            }

            return room.GameState;
        }
    }

    public GameRoom? GetRoom(string roomCode)
    {
        _rooms.TryGetValue(roomCode, out var room);
        return room;
    }

    public GameRoom? GetRoomByConnectionId(string connectionId)
    {
        return _rooms.Values.FirstOrDefault(r => r.GameState.Players.ContainsKey(connectionId));
    }

    public bool StartGame(string roomCode, string connectionId)
    {
        lock (_lock)
        {
            var room = GetRoom(roomCode);
            if (room == null || room.GameState.HostConnectionId != connectionId)
                return false;

            if (!room.GameState.CanStartGame())
                return false;

            room.GameState.Status = GameStatus.InProgress;
            room.GameState.GameStartTime = DateTime.UtcNow;
            room.GameState.AssignRoles();
            room.GameState.SpawnPlayers();

            return true;
        }
    }

    public bool ProcessMove(string roomCode, string connectionId, string direction)
    {
        lock (_lock)
        {
            var room = GetRoom(roomCode);
            if (room == null || room.GameState.Status != GameStatus.InProgress)
                return false;

            if (!room.GameState.Players.TryGetValue(connectionId, out var player))
                return false;

            if (!player.IsAlive)
                return false;

            // Calculate new position
            var newPos = new Position(player.Position.X, player.Position.Y);
            
            switch (direction.ToLower())
            {
                case "up":
                    newPos.Y -= 1;
                    break;
                case "down":
                    newPos.Y += 1;
                    break;
                case "left":
                    newPos.X -= 1;
                    break;
                case "right":
                    newPos.X += 1;
                    break;
                default:
                    return false;
            }

            // Validate move
            if (!room.GameState.Map.IsValidMove(newPos))
                return false;

            // Update position
            player.Position = newPos;
            player.LastMoveTime = DateTime.UtcNow;

            // If runner, collect food
            if (player.Role == PlayerRole.Runner && room.GameState.Map.HasFood(newPos))
            {
                room.GameState.Map.CollectFood(newPos);
                player.Score += 10;
                room.GameState.RemainingFood = room.GameState.Map.CountRemainingFood();
            }

            // Check collisions
            CheckCollisions(room);

            // Check win conditions
            CheckWinConditions(room);

            return true;
        }
    }

    private void CheckCollisions(GameRoom room)
    {
        var chasers = room.GameState.Players.Values.Where(p => p.Role == PlayerRole.Chaser && p.IsAlive).ToList();
        var runners = room.GameState.Players.Values.Where(p => p.Role == PlayerRole.Runner && p.IsAlive).ToList();

        foreach (var chaser in chasers)
        {
            foreach (var runner in runners)
            {
                // Check if chaser and runner are on same position
                if (chaser.Position.Equals(runner.Position))
                {
                    runner.IsAlive = false;
                }
            }
        }
    }

    private void CheckWinConditions(GameRoom room)
    {
        // Runners win if all food collected
        if (room.GameState.RemainingFood == 0)
        {
            room.GameState.Status = GameStatus.RunnersWin;
            return;
        }

        // Chasers win if all runners are dead
        var aliveRunners = room.GameState.Players.Values
            .Count(p => p.Role == PlayerRole.Runner && p.IsAlive);

        if (aliveRunners == 0)
        {
            room.GameState.Status = GameStatus.ChasersWin;
        }
    }

    public List<GameRoom> GetAllRooms()
    {
        return _rooms.Values.ToList();
    }
}