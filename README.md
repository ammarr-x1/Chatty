# 🗨️ Chatty - Real-Time Chat & Multiplayer Game

Chatty is a versatile real-time application that combines direct and group messaging with a fun, multiplayer "Runner vs Chaser" game. Built with Blazor Server and SignalR, it offers a seamless experience for both social interaction and competitive gameplay.

![Status](https://img.shields.io/badge/Status-Active-brightgreen)
![Tech](https://img.shields.io/badge/Tech-Blazor%20|%20SignalR%20|%20C%23-blue)

## 🚀 Features

- **Real-Time Chat**: Direct messaging between users and room-specific group chats.
- **Multiplayer Game (Pacman Chase)**: Join a lobby, wait for players, and compete in a dynamic, fog-of-war-enabled arena.
- **Lobby System**: Seamlessly transition from a chat room to a game lobby using unique room codes.
- **Dynamic Roles**: Players are randomized into **Runners** (Pacman) and **Chasers** (Ghosts) when the game starts.
- **Fog of War**: Chasers have limited vision, adding a tactical layer to the hunt.
- **Session Management**: Persistent user sessions allowing players to stay connected throughout the chat-to-game flow.
- **Optimized Performance**:
  - Incremental food tracking and tick-based movement synchronization.
  - Head-on collision detection and optimized SignalR state updates.

## 🛠️ Tech Stack

- **Frontend/Backend**: [Blazor Server](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
- **Real-Time Engine**: [SignalR](https://dotnet.microsoft.com/en-us/apps/aspnet/signalr)
- **Styling**: Vanilla CSS for a premium, custom-tailored look.
- **Dependency Injection**: Scoped and Singleton services for managing user sessions and chat/game states.

## 🧬 PDC Concepts (Parallel & Distributed Computing)

This project demonstrates key PDC principles:
- **Client-Server Architecture**: The server acts as the "Single Source of Truth" for both chat logs and game state.
- **Concurrency Control**: Thread-safe collections and locking mechanisms to handle simultaneous inputs from multiple users.
- **Asynchronous Communication**: SignalR Hubs facilitate low-latency, non-blocking communication between distributed clients.
- **State Synchronization**: Regular game ticks and delta updates ensure a consistent view across all distributed nodes.

## 🏁 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

### Installation & Run
1. **Clone the repository**:
   ```powershell
   git clone https://github.com/ammarr-x1/Chatty.git
   cd Chatty
   ```

2. **Run the application**:
   ```powershell
   dotnet run
   ```

3. **Access Chatty**:
   Open your browser and navigate to `http://localhost:5286`.

## 🎮 Controls
- **Chat**: Enter usernames to start direct chats or use room codes for group sessions.
- **Game Movement**: `WASD` or `Arrow Keys`.
- **Game Lobby**: Share your **Room Code** with friends to play together!

---
Developed as part of a Parallel and Distributed Computing project.
