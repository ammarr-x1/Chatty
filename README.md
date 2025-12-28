# 🎮 Pacman Chase - Multiplayer Web Game

Pacman Chase is a real-time, multiplayer "Runner vs Chaser" game built with Blazor Server and SignalR. Players join a lobby, chat in real-time, and compete in a dynamic, fog-of-war-enabled Pacman arena.

![Game Preview](https://img.shields.io/badge/Status-Active-brightgreen)
![Tech](https://img.shields.io/badge/Tech-Blazor%20|%20SignalR%20|%20MongoDB-blue)

## 🚀 Features

- **Real-Time Multiplayer**: Seamless gameplay synchronized across all clients using WebSockets (SignalR).
- **Lobby System**: Create or join rooms with unique codes.
- **Dynamic Roles**: Players are randomized into **Runners** (Pacman) and **Chasers** (Ghosts).
- **Fog of War**: Chasers have limited vision, adding a tactical layer to the hunt.
- **Interactive Chat**: Room-specific chat for player coordination.
- **Optimized Performance**:
  - Incremental food tracking (O(1) complexity).
  - Head-on collision detection (prevents "ghosting" through opponents).
  - Tick-based movement synchronized at 160ms intervals.
- **Persistence**: User accounts managed via MongoDB Atlas.

## 🛠️ Tech Stack

- **Frontend/Backend**: [Blazor Server](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
- **Real-Time Engine**: [SignalR](https://dotnet.microsoft.com/en-us/apps/aspnet/signalr)
- **Database**: [MongoDB Atlas](https://www.mongodb.com/atlas/database)
- **Styling**: Vanilla CSS with modern aesthetics.
- **Communication**: WebSockets for low-latency state updates.

## 🧬 PDC Concepts (Parallel & Distributed Computing)

This project was designed to demonstrate key PDC principles:
- **Client-Server Architecture**: The server acts as the "Single Source of Truth" for game state.
- **Concurrency Control**: Use of thread-safe collections (`ConcurrentDictionary`) and lock mechanisms to handle simultaneous player inputs.
- **State Synchronization**: Delta updates sent to all clients every game tick (160ms) to ensure consistent views.
- **Message Passing**: SignalR Hubs facilitate asynchronous communication between distributed nodes (players).
- **Distributed Logic**: Movement verification and collision logic handled on the server to prevent cheating.

## 🏁 Getting Started

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or higher.
- A MongoDB Atlas connection string (update in `appsettings.json`).

### Installation & Run
1. **Clone the repository**:
   ```powershell
   git clone https://github.com/ammarr-x1/Chatty.git
   cd Chatty
   ```

2. **Restore dependencies**:
   ```powershell
   dotnet restore
   ```

3. **Run the application**:
   ```powershell
   dotnet run
   ```

4. **Access the game**:
   Open your browser and navigate to `http://localhost:5286`.

## 🎮 Controls
- **Movement**: `WASD` or `Arrow Keys`.
- **Chat**: Type in the lobby and press `Enter`.
- **Lobby**: Share your **Room Code** with friends to play together!

---
Developed as part of a Parallel and Distributed Computing project.
