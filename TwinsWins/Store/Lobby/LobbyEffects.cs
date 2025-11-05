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

    private async Task StartSignalRConnection(IDispatcher dispatcher)
    {
        try
        {
            // Get the API base URL from HttpClient
            var apiBaseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7103";
            var hubUrl = $"{apiBaseUrl}/gamehub";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            // Listen for new games added to lobby
            _hubConnection.On<GameLobby>("ReceiveNewGame", (game) =>
            {
                dispatcher.Dispatch(new GameAddedToLobbyAction(game));
            });

            // Listen for games removed from lobby
            _hubConnection.On<GameLobby>("ReceiveDeleteGame", (game) =>
            {
                dispatcher.Dispatch(new GameRemovedFromLobbyAction(game.Id));
            });

            // Start connection
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LobbyGamesLoadFailedAction($"SignalR connection failed: {ex.Message}"));
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
            await StartSignalRConnection(dispatcher);
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