using Microsoft.AspNetCore.SignalR;

namespace TwinsWins.Api.Hubs;

/// <summary>
/// SignalR hub for real-time game lobby updates
/// </summary>
public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);

        if (exception != null)
        {
            _logger.LogError(exception, "Client disconnected with error");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific game room (optional, for game-specific updates)
    /// </summary>
    public async Task JoinGameRoom(long gameId)
    {
        var roomName = $"game_{gameId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        _logger.LogInformation(
            "Client {ConnectionId} joined game room {GameId}",
            Context.ConnectionId, gameId);
    }

    /// <summary>
    /// Leave a game room
    /// </summary>
    public async Task LeaveGameRoom(long gameId)
    {
        var roomName = $"game_{gameId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        _logger.LogInformation(
            "Client {ConnectionId} left game room {GameId}",
            Context.ConnectionId, gameId);
    }

    /// <summary>
    /// Send a message to all clients in a specific game room
    /// </summary>
    public async Task SendToGameRoom(long gameId, string message, object data)
    {
        var roomName = $"game_{gameId}";
        await Clients.Group(roomName).SendAsync(message, data);
        _logger.LogInformation(
            "Message {Message} sent to game room {GameId}",
            message, gameId);
    }
}