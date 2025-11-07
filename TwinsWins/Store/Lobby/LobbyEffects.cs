using Fluxor;
using System.Net.Http.Json;
using TwinsWins.Shared.DTOs;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Lobby;

/// <summary>
/// Effects for lobby operations
/// All Effect methods MUST have exactly this signature when using [EffectMethod]:
/// public async Task HandleAction(ActionType action, IDispatcher dispatcher)
/// </summary>
public class LobbyEffects
{
    private readonly HttpClient _httpClient;

    public LobbyEffects(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Effect to load available games from lobby
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadLobbyGames(LoadLobbyGamesAction action, IDispatcher dispatcher)
    {
        try
        {
            var games = await _httpClient.GetFromJsonAsync<List<GameLobby>>("/api/lobby/games");

            if (games != null)
            {
                dispatcher.Dispatch(new LoadLobbyGamesSuccessAction(games));
            }
            else
            {
                dispatcher.Dispatch(new LoadLobbyGamesFailureAction("No games found"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadLobbyGamesFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect to load available games (alternative)
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadGames(LoadGamesAction action, IDispatcher dispatcher)
    {
        try
        {
            var games = await _httpClient.GetFromJsonAsync<List<GameLobby>>("/api/lobby/games");

            if (games != null)
            {
                dispatcher.Dispatch(new LoadGamesSuccessAction(games));
            }
            else
            {
                dispatcher.Dispatch(new LoadGamesFailureAction("No games found"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadGamesFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect to load lobby statistics
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadStats(LoadStatsAction action, IDispatcher dispatcher)
    {
        try
        {
            var stats = await _httpClient.GetFromJsonAsync<LobbyStatsResponse>("/api/lobby/stats");

            if (stats != null)
            {
                dispatcher.Dispatch(new LoadStatsSuccessAction(stats));
            }
            else
            {
                dispatcher.Dispatch(new LoadStatsFailureAction("Failed to load stats"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadStatsFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect to create a new paid game
    /// </summary>
    [EffectMethod]
    public async Task HandleCreateGame(CreateGameAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new CreateGameRequest
            {
                WalletAddress = action.WalletAddress,
                Stake = action.Stake
            };

            var response = await _httpClient.PostAsJsonAsync("/api/game/create", request);
            var result = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

            if (response.IsSuccessStatusCode && result != null)
            {
                var cells = result.Cells.Select(c => new Cell
                {
                    Id = c.Id,
                    ImagePath = c.ImagePath
                }).ToList();

                dispatcher.Dispatch(new CreateGameSuccessAction(
                    GameId: result.GameId,
                    Cells: cells,
                    ImageIdMap: result.ImageIdMap
                ));
            }
            else
            {
                dispatcher.Dispatch(new CreateGameFailureAction("Failed to create game"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CreateGameFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect to join an existing game
    /// </summary>
    [EffectMethod]
    public async Task HandleJoinGame(JoinGameAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new JoinGameRequest
            {
                GameId = action.GameId,
                WalletAddress = action.WalletAddress
            };

            var response = await _httpClient.PostAsJsonAsync("/api/game/join", request);
            var result = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

            if (response.IsSuccessStatusCode && result != null)
            {
                var cells = result.Cells.Select(c => new Cell
                {
                    Id = c.Id,
                    ImagePath = c.ImagePath
                }).ToList();

                dispatcher.Dispatch(new JoinGameSuccessAction(
                    GameId: result.GameId,
                    Cells: cells,
                    ImageIdMap: result.ImageIdMap
                ));
            }
            else
            {
                dispatcher.Dispatch(new JoinGameFailureAction("Failed to join game"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new JoinGameFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect to start auto-refresh of lobby data
    /// </summary>
    [EffectMethod]
    public async Task HandleStartAutoRefresh(StartAutoRefreshAction action, IDispatcher dispatcher)
    {
        while (true)
        {
            await Task.Delay(5000); // Refresh every 5 seconds

            // Dispatch action to reload games and stats
            dispatcher.Dispatch(new LoadGamesAction());
            dispatcher.Dispatch(new LoadStatsAction());
        }
    }
}