// Hubs/GameHub.cs

using Microsoft.AspNetCore.SignalR;
using PacmanMultiplayer.Models;
using PacmanMultiplayer.Services;

namespace PacmanMultiplayer.Hubs;

public class GameHub : Hub
{
    private readonly GameManager _gameManager;

    public GameHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task<bool> JoinRoom(string roomCode, string username)
    {
        var room = _gameManager.CreateOrJoinRoom(roomCode, Context.ConnectionId, username);
        
        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Room is full or game already started");
            return false;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        
        await Clients.Group(roomCode).SendAsync("PlayerJoined", username);
        await Clients.Group(roomCode).SendAsync("GameStateUpdated", room.GameState);
        
        return true;
    }

    public async Task<GameState?> ReconnectToGame(string roomCode, string username)
    {
        var state = _gameManager.ReconnectPlayer(roomCode, username, Context.ConnectionId);
        
        if (state == null)
        {
            await Clients.Caller.SendAsync("Error", "Failed to reconnect: Room or player not found");
            return null;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        
        return state;
    }

    public async Task StartGame(string roomCode)
    {
        var success = _gameManager.StartGame(roomCode, Context.ConnectionId);
        
        if (success)
        {
            var room = _gameManager.GetRoom(roomCode);
            if (room != null)
            {
                await Clients.Group(roomCode).SendAsync("GameStarted");
                await Clients.Group(roomCode).SendAsync("GameStateUpdated", room.GameState);
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Cannot start game. Need 2-5 players.");
        }
    }

    public async Task SetDirection(string direction)
    {
        var room = _gameManager.GetRoomByConnectionId(Context.ConnectionId);
        
        if (room == null)
            return;

        // Just set the direction - game loop will handle movement
        _gameManager.SetPlayerDirection(room.RoomCode, Context.ConnectionId, direction);
        
        // No need to broadcast here - game loop handles it
    }

    public async Task LeaveRoom()
    {
        var room = _gameManager.GetRoomByConnectionId(Context.ConnectionId);
        
        if (room != null)
        {
            var player = room.GameState.Players.GetValueOrDefault(Context.ConnectionId);
            var username = player?.Username ?? "Unknown";
            
            _gameManager.RemovePlayer(Context.ConnectionId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.RoomCode);
            await Clients.Group(room.RoomCode).SendAsync("PlayerLeft", username);
            
            var updatedRoom = _gameManager.GetRoom(room.RoomCode);
            if (updatedRoom != null)
            {
                await Clients.Group(room.RoomCode).SendAsync("GameStateUpdated", updatedRoom.GameState);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveRoom();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendChatMessage(string message)
    {
        var room = _gameManager.GetRoomByConnectionId(Context.ConnectionId);
        
        if (room != null)
        {
            var player = room.GameState.Players.GetValueOrDefault(Context.ConnectionId);
            var username = player?.Username ?? "Unknown";
            
            await Clients.Group(room.RoomCode).SendAsync("ReceiveChatMessage", username, message);
        }
    }
}