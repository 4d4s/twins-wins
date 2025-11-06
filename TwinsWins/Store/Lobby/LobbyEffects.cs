using Fluxor;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Lobby;

public class LobbyEffects : IDisposable
{
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;

    public LobbyEffects(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task EnsureConnectedAsync(IDispatcher dispatcher)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
            return;

        if (_hubConnection?.State == HubConnectionState.Connecting)
        {
            // Wait for connection to complete
            while (_hubConnection.State == HubConnectionState.Connecting)
                await Task.Delay(100);
            return;
        }

        try
        {
            var apiBaseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/')
                ?? "https://localhost:7103";
            var hubUrl = $"{apiBaseUrl}/gamehub";

            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect(new[]
                    {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(10)
                    })
                    .Build();

                _hubConnection.On<GameLobby>("ReceiveNewGame", (game) =>
                {
                    dispatcher.Dispatch(new GameAddedToLobbyAction(game));
                });

                _hubConnection.On<GameLobby>("ReceiveDeleteGame", (game) =>
                {
                    dispatcher.Dispatch(new GameRemovedFromLobbyAction(game.Id));
                });
            }

            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            // Log but don't fail - app can work without real-time updates
            Console.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }

    [EffectMethod]
    public async Task HandleLoadLobbyGames(LoadLobbyGamesAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetLobbyLoadingAction(true));

        try
        {
            var games = await _httpClient.GetFromJsonAsync<List<GameLobby>>("/api/lobby/games");

            if (games != null)
            {
                dispatcher.Dispatch(new LobbyGamesLoadedAction(games));
            }
            else
            {
                dispatcher.Dispatch(new LobbyGamesLoadedAction(new List<GameLobby>()));
            }

            // Start SignalR connection after initial load
            await EnsureConnectedAsync(dispatcher);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LobbyGamesLoadFailedAction(ex.Message));
        }
    }

    public void Dispose()
    {
        _hubConnection?.DisposeAsync();
    }
}