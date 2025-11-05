using Fluxor;
using System.Net.Http.Json;
using TwinsWins.Shared.DTOs;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// Effects handle side effects (API calls, timers, etc.)
/// They dispatch new actions based on results
/// </summary>
public class GameEffects
{
    private readonly HttpClient _httpClient;
    private System.Timers.Timer? _countdownTimer;
    private System.Timers.Timer? _gameTimer;

    public GameEffects(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ============ Game Initialization ============

    [EffectMethod]
    public async Task HandleInitFreeGame(InitFreeGameAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetLoadingAction(true));

        try
        {
            var response = await _httpClient.PostAsync("/api/game/init-free", null);
            response.EnsureSuccessStatusCode();

            var gameResponse = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

            if (gameResponse != null)
            {
                var cells = gameResponse.Cells.Select(c => new Cell
                {
                    Id = c.Id,
                    ImagePath = c.ImagePath,
                    IsMatched = false,
                    IsRevealed = false
                }).ToList();

                dispatcher.Dispatch(new GameInitializedAction(
                    null,
                    cells,
                    gameResponse.ImageIdMap
                ));

                dispatcher.Dispatch(new StartCountdownAction());
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new GameInitializationFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleInitPaidGame(InitPaidGameAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetLoadingAction(true));

        try
        {
            var request = new CreateGameRequest
            {
                WalletAddress = action.WalletAddress,
                Stake = action.Stake
            };

            var response = await _httpClient.PostAsJsonAsync("/api/game/init-paid", request);
            response.EnsureSuccessStatusCode();

            var gameResponse = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

            if (gameResponse != null)
            {
                var cells = gameResponse.Cells.Select(c => new Cell
                {
                    Id = c.Id,
                    ImagePath = c.ImagePath,
                    IsMatched = false,
                    IsRevealed = false
                }).ToList();

                dispatcher.Dispatch(new GameInitializedAction(
                    gameResponse.GameId,
                    cells,
                    gameResponse.ImageIdMap
                ));

                dispatcher.Dispatch(new StartCountdownAction());
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new GameInitializationFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleJoinPaidGame(JoinPaidGameAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetLoadingAction(true));

        try
        {
            var request = new JoinGameRequest
            {
                GameId = action.GameId,
                WalletAddress = action.WalletAddress
            };

            var response = await _httpClient.PostAsJsonAsync("/api/game/join", request);
            response.EnsureSuccessStatusCode();

            var gameResponse = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

            if (gameResponse != null)
            {
                var cells = gameResponse.Cells.Select(c => new Cell
                {
                    Id = c.Id,
                    ImagePath = c.ImagePath,
                    IsMatched = false,
                    IsRevealed = false
                }).ToList();

                dispatcher.Dispatch(new GameInitializedAction(
                    action.GameId,
                    cells,
                    gameResponse.ImageIdMap
                ));

                dispatcher.Dispatch(new StartCountdownAction());
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new GameInitializationFailedAction(ex.Message));
        }
    }

    // ============ Countdown Timer ============

    [EffectMethod]
    public Task HandleStartCountdown(StartCountdownAction action, IDispatcher dispatcher)
    {
        _countdownTimer?.Dispose();
        _countdownTimer = new System.Timers.Timer(1000); // 1 second

        var countdown = 3;
        _countdownTimer.Elapsed += (sender, e) =>
        {
            if (countdown > 1)
            {
                countdown--;
                dispatcher.Dispatch(new CountdownTickAction(countdown));
            }
            else
            {
                _countdownTimer?.Stop();
                _countdownTimer?.Dispose();
                dispatcher.Dispatch(new CountdownCompleteAction());
                dispatcher.Dispatch(new StartGameAction());
            }
        };

        _countdownTimer.Start();
        return Task.CompletedTask;
    }

    // ============ Game Timer ============

    [EffectMethod]
    public Task HandleStartGame(StartGameAction action, IDispatcher dispatcher)
    {
        _gameTimer?.Dispose();
        _gameTimer = new System.Timers.Timer(1000); // 1 second

        var timeRemaining = 60;
        _gameTimer.Elapsed += (sender, e) =>
        {
            if (timeRemaining > 0)
            {
                timeRemaining--;
                dispatcher.Dispatch(new GameTickAction(timeRemaining));
            }
            else
            {
                _gameTimer?.Stop();
                _gameTimer?.Dispose();

                // Get final score from state - will need to retrieve it
                dispatcher.Dispatch(new EndGameAction(0)); // Score will be updated from state
            }
        };

        _gameTimer.Start();
        return Task.CompletedTask;
    }

    // ============ Cell Click & Match Logic ============

    [EffectMethod]
    public async Task HandleCellClicked(CellClickedAction action, IDispatcher dispatcher)
    {
        // Reveal the cell
        dispatcher.Dispatch(new RevealCellAction(action.CellId));

        // Wait a bit then check for match
        await Task.Delay(100);
        dispatcher.Dispatch(new CheckMatchAction());
    }

    [EffectMethod(typeof(CheckMatchAction))]
    public async Task HandleCheckMatch(IDispatcher dispatcher, IState<GameState> state)
    {
        var gameState = state.Value;

        if (gameState.FirstSelectedCell != null && gameState.SecondSelectedCell != null)
        {
            var cell1Id = gameState.FirstSelectedCell.Id;
            var cell2Id = gameState.SecondSelectedCell.Id;

            // Check if they match using the image map
            var isMatch = gameState.ImageIdMap.ContainsKey(cell1Id) &&
                         gameState.ImageIdMap[cell1Id] == cell2Id;

            // Wait a moment so user can see both cards
            await Task.Delay(800);

            if (isMatch)
            {
                // Calculate points based on time elapsed
                var points = CalculatePoints(gameState.GameStartTime, gameState.TimeRemaining);
                dispatcher.Dispatch(new MatchFoundAction(cell1Id, cell2Id, points));
            }
            else
            {
                dispatcher.Dispatch(new MatchFailedAction(cell1Id, cell2Id, -100));
            }

            // Check if game is complete
            if (gameState.IsGameComplete)
            {
                dispatcher.Dispatch(new EndGameAction(gameState.Score));
            }
        }
    }

    // ============ Score Submission ============

    [EffectMethod]
    public async Task HandleSubmitScore(SubmitScoreAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetLoadingAction(true));

        try
        {
            var request = new SubmitScoreRequest
            {
                GameId = action.GameId,
                WalletAddress = action.WalletAddress,
                Score = action.Score,
                TimeElapsed = 60 // Will be calculated from state
            };

            var response = await _httpClient.PostAsJsonAsync("/api/game/submit-score", request);
            response.EnsureSuccessStatusCode();

            dispatcher.Dispatch(new ScoreSubmittedAction());
            dispatcher.Dispatch(new SetLoadingAction(false));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ScoreSubmissionFailedAction(ex.Message));
        }
    }

    // ============ Helper Methods ============

    private int CalculatePoints(DateTime? startTime, int timeRemaining)
    {
        if (startTime == null) return 0;

        var elapsedTime = (DateTime.UtcNow - startTime.Value).TotalSeconds;
        var timeRatio = elapsedTime / 60.0;
        const int maxPoints = 1000;

        var points = Math.Floor(maxPoints * (1 - timeRatio));
        return Math.Max((int)points, 100); // Minimum 100 points per match
    }

    public void Dispose()
    {
        _countdownTimer?.Dispose();
        _gameTimer?.Dispose();
    }
}