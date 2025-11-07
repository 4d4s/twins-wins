using Fluxor;
using System.Net.Http.Json;
using TwinsWins.Shared.DTOs;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// Effects handle side effects like API calls, timers, and async operations
/// All Effect methods MUST have exactly this signature when using [EffectMethod]:
/// public async Task HandleAction(ActionType action, IDispatcher dispatcher)
/// </summary>
public class GameEffects
{
    private readonly HttpClient _httpClient;
    private CancellationTokenSource? _timerCancellation;
    private CancellationTokenSource? _countdownCancellation;

    public GameEffects(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Effect to initialize a free practice game
    /// </summary>
    [EffectMethod]
    public async Task HandleInitFreeGame(InitFreeGameAction action, IDispatcher dispatcher)
    {
        try
        {
            // Call API to initialize free game
            var response = await _httpClient.GetFromJsonAsync<CreateGameResponse>("/api/game/init-free");

            if (response != null)
            {
                var cells = response.Cells.Select(c => new Cell 
                { 
                    Id = c.Id, 
                    ImagePath = c.ImagePath 
                }).ToList();

                dispatcher.Dispatch(new InitFreeGameSuccessAction(
                    GameId: 0, // Free games have no ID
                    Cells: cells,
                    ImageIdMap: response.ImageIdMap
                ));

                // Start countdown
                await StartCountdown(dispatcher);
            }
            else
            {
                dispatcher.Dispatch(new InitFreeGameFailureAction("Failed to initialize game"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new InitFreeGameFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect to handle countdown before game starts
    /// </summary>
    private async Task StartCountdown(IDispatcher dispatcher)
    {
        _countdownCancellation?.Cancel();
        _countdownCancellation = new CancellationTokenSource();

        try
        {
            for (int i = 3; i > 0; i--)
            {
                dispatcher.Dispatch(new UpdateCountdownAction(i));
                await Task.Delay(1000, _countdownCancellation.Token);
            }

            // Countdown complete - start game
            dispatcher.Dispatch(new CountdownCompleteAction());
            
            // Start timer
            _ = StartTimer(dispatcher);
        }
        catch (TaskCanceledException)
        {
            // Countdown was cancelled
        }
    }

    /// <summary>
    /// Effect to start the game timer
    /// </summary>
    private async Task StartTimer(IDispatcher dispatcher)
    {
        _timerCancellation?.Cancel();
        _timerCancellation = new CancellationTokenSource();

        try
        {
            while (!_timerCancellation.Token.IsCancellationRequested)
            {
                await Task.Delay(1000, _timerCancellation.Token);
                dispatcher.Dispatch(new UpdateTimerAction());
            }
        }
        catch (TaskCanceledException)
        {
            // Timer was cancelled
        }
    }

    /// <summary>
    /// Effect to handle cell click and check for match
    /// </summary>
    [EffectMethod]
    public async Task HandleCellClicked(CellClickedAction action, IDispatcher dispatcher)
    {
        // Wait a moment to show both cards
        await Task.Delay(500);
        
        // The reducer will have updated to show we have 2 selected cells
        // Now we need to check if they match - but we need current state
        // This will be handled by checking in the reducer itself
    }

    /// <summary>
    /// Effect to handle match checking after selection
    /// </summary>
    [EffectMethod]
    public async Task HandleCheckMatch(CheckMatchAction action, IDispatcher dispatcher)
    {
        var state = action.State;

        if (state.SelectedCells.Count != 2)
        {
            return;
        }

        // Add a small delay to show both cards before checking match
        await Task.Delay(800);

        var cell1 = state.SelectedCells[0];
        var cell2 = state.SelectedCells[1];

        // Check if cells have the same image ID mapping
        if (state.ImageIdMap.TryGetValue(cell1.Id, out int matchId) && matchId == cell2.Id)
        {
            // Match found
            dispatcher.Dispatch(new MatchFoundAction());
        }
        else
        {
            // No match - flip cards back
            dispatcher.Dispatch(new NoMatchAction());
        }
    }

    /// <summary>
    /// Effect to submit game result
    /// </summary>
    [EffectMethod]
    public async Task HandleSubmitResult(SubmitGameResultAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new SubmitScoreRequest
            {
                GameId = action.GameId,
                WalletAddress = action.WalletAddress,
                Score = action.Score,
                TimeElapsed = 60 - action.TimeElapsed // Convert remaining time to elapsed
            };

            var response = await _httpClient.PostAsJsonAsync("/api/game/submit-score", request);

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new SubmitGameResultSuccessAction());
            }
            else
            {
                dispatcher.Dispatch(new SubmitGameResultFailureAction("Failed to submit score"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SubmitGameResultFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect to handle game reset
    /// </summary>
    [EffectMethod]
    public Task HandleResetGame(ResetGameAction action, IDispatcher dispatcher)
    {
        // Cancel timers
        _timerCancellation?.Cancel();
        _countdownCancellation?.Cancel();

        return Task.CompletedTask;
    }
}