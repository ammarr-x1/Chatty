// Services/GameLoopService.cs

using Microsoft.AspNetCore.SignalR;
using PacmanMultiplayer.Hubs;

namespace PacmanMultiplayer.Services;

public class GameLoopService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(160); // 5 ticks per second

    public GameLoopService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var gameManager = scope.ServiceProvider.GetRequiredService<GameManager>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();

                    // Get all active games
                    var activeRooms = gameManager.GetActiveRoomCodes().ToList();

                    foreach (var roomCode in activeRooms)
                    {
                        // Update game state
                        gameManager.UpdateGame(roomCode);

                        // Get updated room
                        var room = gameManager.GetRoom(roomCode);
                        if (room != null)
                        {
                            // Broadcast updated state to all players in room
                            await hubContext.Clients.Group(roomCode)
                                .SendAsync("GameStateUpdated", room.GameState, stoppingToken);

                            // Check if game ended
                            if (room.GameState.Status == Models.GameStatus.RunnersWin)
                            {
                                await hubContext.Clients.Group(roomCode)
                                    .SendAsync("GameEnded", "Runners", stoppingToken);
                            }
                            else if (room.GameState.Status == Models.GameStatus.ChasersWin)
                            {
                                await hubContext.Clients.Group(roomCode)
                                    .SendAsync("GameEnded", "Chasers", stoppingToken);
                            }
                        }
                    }
                }

                await Task.Delay(_tickInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in game loop: {ex.Message}");
            }
        }
    }
}