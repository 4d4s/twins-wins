using Fluxor;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using TwinsWins.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace TwinsWins.Client.Store.Lobby;

public class LobbyEffects : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HubConnection _hubConnection;

    public LobbyEffects(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;

        // Setup SignalR connection
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/gamehub"))
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task StartSignalRConnection(IDispatcher dispatcher)
    {
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
        try
        {
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