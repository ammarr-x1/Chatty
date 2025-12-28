// Services/GameManager.cs

using System.Collections.Concurrent;
using PacmanMultiplayer.Models;

namespace PacmanMultiplayer.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly object _lock = new object();

    public GameRoom? CreateOrJoinRoom(string roomCode, string connectionId, string username)
    {
        lock (_lock)
        {
            var room = _rooms.GetOrAdd(roomCode, code => new GameRoom(code));

            if (room.GameState.Players.Count >= 5)
                return null;

            if (room.GameState.Status == GameStatus.InProgress)
                return null;

            var player = new PlayerState
            {
                ConnectionId = connectionId,
                Username = username,
                IsAlive = true
            };

            room.GameState.Players[connectionId] = player;

            if (room.GameState.Players.Count == 1)
            {
                room.GameState.HostConnectionId = connectionId;
            }

            return room;
        }
    }

    public GameState? ReconnectPlayer(string roomCode, string username, string newConnectionId)
    {
        lock (_lock)
        {
            var room = GetRoom(roomCode);
            if (room == null) return null;

            // Find player by username
            var existingPlayer = room.GameState.Players.Values
                .FirstOrDefault(p => p.Username == username);

            if (existingPlayer != null)
            {
                // Update connection ID
                var oldConnectionId = existingPlayer.ConnectionId;
                existingPlayer.ConnectionId = newConnectionId;
                
                room.GameState.Players.Remove(oldConnectionId);
                room.GameState.Players[newConnectionId] = existingPlayer;

                // Update host if needed
                if (room.GameState.HostConnectionId == oldConnectionId)
                {
                    room.GameState.HostConnectionId = newConnectionId;
                }
            }

            return room.GameState;
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
                    // Don't remove player if game is in progress
                    if (room.GameState.Status == GameStatus.InProgress)
                    {
                        return false;
                    }

                    room.GameState.Players.Remove(connectionId);

                    if (room.GameState.Players.Count == 0)
                    {
                        _rooms.TryRemove(room.RoomCode, out _);
                    }
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

    public bool SetPlayerDirection(string roomCode, string connectionId, string directionStr)
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

            // Parse direction
            Direction newDirection = directionStr.ToLower() switch
            {
                "up" => Direction.Up,
                "down" => Direction.Down,
                "left" => Direction.Left,
                "right" => Direction.Right,
                _ => Direction.None
            };

            if (newDirection == Direction.None)
                return false;

            // Check if new direction is opposite to current direction (reversing)
            if (!player.IsStopped && player.Role == PlayerRole.Runner)
            {
                bool isReverse = (player.CurrentDirection == Direction.Up && newDirection == Direction.Down) ||
                                (player.CurrentDirection == Direction.Down && newDirection == Direction.Up) ||
                                (player.CurrentDirection == Direction.Left && newDirection == Direction.Right) ||
                                (player.CurrentDirection == Direction.Right && newDirection == Direction.Left);

                if (isReverse)
                    return false; // Runners can't reverse unless stopped
            }

            // Check if the new direction is valid (not a wall)
            var nextPos = player.Position.GetNextPosition(newDirection);
            if (!room.GameState.Map.IsValidMove(nextPos))
            {
                // Can't set direction toward a wall
                return false;
            }

            // Queue the direction change
            player.NextDirection = newDirection;
            
            // If player is stopped, apply immediately
            if (player.IsStopped)
            {
                player.CurrentDirection = newDirection;
                player.IsStopped = false;
            }

            return true;
        }
    }

    public void UpdateGame(string roomCode)
    {
        lock (_lock)
        {
            var room = GetRoom(roomCode);
            if (room == null || room.GameState.Status != GameStatus.InProgress)
                return;

            // Move all players
            foreach (var player in room.GameState.Players.Values.Where(p => p.IsAlive))
            {
                MovePlayer(room, player);
            }

            // Check collisions after all moves
            CheckCollisions(room);

            // Check win conditions
            CheckWinConditions(room);
        }
    }

    private void MovePlayer(GameRoom room, PlayerState player)
    {
        // If player has queued direction, try to apply it
        if (player.NextDirection != Direction.None && player.NextDirection != player.CurrentDirection)
        {
            var nextPos = player.Position.GetNextPosition(player.NextDirection);
            if (room.GameState.Map.IsValidMove(nextPos))
            {
                // Valid direction change
                player.CurrentDirection = player.NextDirection;
                player.NextDirection = Direction.None;
                player.IsStopped = false;
            }
        }

        // If stopped, don't move
        if (player.IsStopped || player.CurrentDirection == Direction.None)
            return;

        // Calculate next position based on current direction
        var newPos = player.Position.GetNextPosition(player.CurrentDirection);

        // Check if move is valid
        if (!room.GameState.Map.IsValidMove(newPos))
        {
            // Hit a wall - stop the player
            player.IsStopped = true;
            player.CurrentDirection = Direction.None;
            return;
        }

        // Move is valid - update position
        player.Position = newPos;
        player.LastMoveTime = DateTime.UtcNow;

        // If runner, collect food
        if (player.Role == PlayerRole.Runner && room.GameState.Map.HasFood(newPos))
        {
            room.GameState.Map.CollectFood(newPos);
            player.Score += 10;
            room.GameState.RemainingFood = room.GameState.Map.CountRemainingFood();
        }
    }

    private void CheckCollisions(GameRoom room)
    {
        var chasers = room.GameState.Players.Values
            .Where(p => p.Role == PlayerRole.Chaser && p.IsAlive).ToList();
        var runners = room.GameState.Players.Values
            .Where(p => p.Role == PlayerRole.Runner && p.IsAlive).ToList();

        foreach (var chaser in chasers)
        {
            foreach (var runner in runners)
            {
                if (chaser.Position.Equals(runner.Position))
                {
                    runner.IsAlive = false;
                    runner.IsStopped = true;
                    runner.CurrentDirection = Direction.None;
                }
            }
        }
    }

    private void CheckWinConditions(GameRoom room)
    {
        if (room.GameState.RemainingFood == 0)
        {
            room.GameState.Status = GameStatus.RunnersWin;
            return;
        }

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

    public IEnumerable<string> GetActiveRoomCodes()
    {
        return _rooms.Values
            .Where(r => r.GameState.Status == GameStatus.InProgress)
            .Select(r => r.RoomCode);
    }
}