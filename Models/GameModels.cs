// Models/GameModels.cs

using System.Text.Json.Serialization;

namespace PacmanMultiplayer.Models;

public enum CellType
{
    Empty,
    Wall,
    Food
}

public enum PlayerRole
{
    Runner,  // Pacman
    Chaser   // Ghost
}

public enum GameStatus
{
    Waiting,
    InProgress,
    RunnersWin,
    ChasersWin
}

public class Position
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Position pos && pos.X == X && pos.Y == Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public double DistanceTo(Position other)
    {
        return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    }
}

public class PlayerState
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Position Position { get; set; } = new Position(0, 0);
    public PlayerRole Role { get; set; }
    public bool IsAlive { get; set; } = true;
    public int VisionRadius { get; set; } = 5;
    public string Color { get; set; } = "yellow"; // Visual color identifier
    public int Score { get; set; } = 0; // For runners collecting food
    public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;
}

public class GameMap
{
    private static string[] MapLayout = new[]
    {
        "##############################",
        "#.............##.............#",
        "#.####.######.##.######.####.#",
        "#............................#",
        "#.####.###.########.###.####.#",
        "#......###....##....###......#",
        "######.######.##.######.######",
        "#   ##.###..........###.##   #",
        "######.###.########.###.######",
        "#..........##    ##..........#",
        "######.###.###  ###.###.######",
        "#   ##.###..........###.##   #",
        "######.######.##.######.######",
        "#......###....##....###......#",
        "#.####.###.########.###.####.#",
        "#............................#",
        "#.####.######.##.######.####.#",
        "#............##..............#",
        "##############################",
    };

    public int Width { get; }
    public int Height { get; }
    
    [JsonIgnore]
    public CellType[,] Grid { get; private set; }

    // Serializable version of Grid for SignalR
    public CellType[][] GridData
    {
        get
        {
            var result = new CellType[Height][];
            for (int y = 0; y < Height; y++)
            {
                result[y] = new CellType[Width];
                for (int x = 0; x < Width; x++)
                {
                    result[y][x] = Grid[x, y];
                }
            }
            return result;
        }
    }

    public GameMap()
    {
        Height = MapLayout.Length;
        Width = MapLayout[0].Length;
        Grid = new CellType[Width, Height];
        InitializeMap();
    }

    private void InitializeMap()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                char cell = MapLayout[y][x];
                Grid[x, y] = cell switch
                {
                    '#' => CellType.Wall,
                    '.' => CellType.Food,
                    _ => CellType.Empty
                };
            }
        }
    }

    public bool IsValidMove(Position pos)
    {
        if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height)
            return false;

        return Grid[pos.X, pos.Y] != CellType.Wall;
    }

    public bool HasFood(Position pos)
    {
        if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height)
            return false;

        return Grid[pos.X, pos.Y] == CellType.Food;
    }

    public void CollectFood(Position pos)
    {
        if (HasFood(pos))
        {
            Grid[pos.X, pos.Y] = CellType.Empty;
        }
    }

    public int CountRemainingFood()
    {
        int count = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Grid[x, y] == CellType.Food)
                    count++;
            }
        }
        return count;
    }
}

public class GameState
{
    public string RoomCode { get; set; } = string.Empty;
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    public GameMap Map { get; set; } = new GameMap();
    public Dictionary<string, PlayerState> Players { get; set; } = new Dictionary<string, PlayerState>();
    public int TotalFood { get; private set; }
    public int RemainingFood { get; set; }
    public DateTime GameStartTime { get; set; }
    public string HostConnectionId { get; set; } = string.Empty;

    public GameState(string roomCode)
    {
        RoomCode = roomCode;
        TotalFood = Map.CountRemainingFood();
        RemainingFood = TotalFood;
    }

    public bool CanStartGame()
    {
        // Need at least 2 players (1 runner, 1 chaser)
        return Players.Count >= 2 && Players.Count <= 5;
    }

    public void AssignRoles()
    {
        var playerList = Players.Values.ToList();
        
        // First player is chaser, rest are runners
        // Or you can do: first 1-2 are chasers, rest runners
        int chaserCount = Math.Max(1, playerList.Count / 3); // ~33% chasers

        var colors = new[] { "red", "magenta", "cyan", "orange", "pink" };
        int colorIndex = 0;

        for (int i = 0; i < playerList.Count; i++)
        {
            if (i < chaserCount)
            {
                playerList[i].Role = PlayerRole.Chaser;
                playerList[i].Color = colors[colorIndex++ % colors.Length];
            }
            else
            {
                playerList[i].Role = PlayerRole.Runner;
                playerList[i].Color = "yellow";
            }
        }
    }

    public void SpawnPlayers()
    {
        // Spawn positions for runners and chasers
        var runnerSpawns = new List<Position>
        {
            new Position(1, 1),
            new Position(28, 1),
            new Position(1, 17),
            new Position(28, 17)
        };

        var chaserSpawns = new List<Position>
        {
            new Position(14, 9),
            new Position(15, 9),
            new Position(13, 9)
        };

        int runnerIndex = 0;
        int chaserIndex = 0;

        foreach (var player in Players.Values)
        {
            if (player.Role == PlayerRole.Runner && runnerIndex < runnerSpawns.Count)
            {
                player.Position = runnerSpawns[runnerIndex++];
            }
            else if (player.Role == PlayerRole.Chaser && chaserIndex < chaserSpawns.Count)
            {
                player.Position = chaserSpawns[chaserIndex++];
            }
        }
    }
}

public class GameRoom
{
    public string RoomCode { get; set; } = string.Empty;
    public GameState GameState { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GameRoom(string roomCode)
    {
        RoomCode = roomCode;
        GameState = new GameState(roomCode);
    }
}