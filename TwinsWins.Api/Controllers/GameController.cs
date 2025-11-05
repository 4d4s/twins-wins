using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TwinsWins.Shared.DTOs;
using TwinsWins.Api.Services;
using TwinsWins.Api.Hubs;

namespace TwinsWins.Api.Controllers;

/// <summary>
/// Handles all game-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<GameController> _logger;

    public GameController(
        IGameService gameService,
        IHubContext<GameHub> hubContext,
        ILogger<GameController> logger)
    {
        _gameService = gameService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Initialize a free practice game
    /// </summary>
    /// <returns>Game data with cells and image mapping</returns>
    [HttpPost("init-free")]
    [ProducesResponseType(typeof(CreateGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateGameResponse>> InitFreeGame()
    {
        try
        {
            _logger.LogInformation("Initializing free game");

            var response = await _gameService.CreateFreeGame();

            _logger.LogInformation("Free game created successfully");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating free game");
            return StatusCode(500, new { error = "Failed to create game", details = ex.Message });
        }
    }

    /// <summary>
    /// Initialize a paid game (with cryptocurrency stake)
    /// </summary>
    /// <param name="request">Wallet address and stake amount</param>
    /// <returns>Game data with game ID, cells, and image mapping</returns>
    [HttpPost("init-paid")]
    [ProducesResponseType(typeof(CreateGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateGameResponse>> InitPaidGame([FromBody] CreateGameRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.WalletAddress))
            {
                return BadRequest(new { error = "Wallet address is required" });
            }

            if (request.Stake <= 0)
            {
                return BadRequest(new { error = "Stake must be greater than 0" });
            }

            _logger.LogInformation(
                "Creating paid game for wallet {WalletAddress} with stake {Stake}",
                request.WalletAddress, request.Stake);

            // Create the game
            var response = await _gameService.CreatePaidGame(request.WalletAddress, request.Stake);

            // Broadcast to all clients that a new game is available
            var lobby = await _gameService.GetLobbyGame(response.GameId);
            await _hubContext.Clients.All.SendAsync("ReceiveNewGame", lobby);

            _logger.LogInformation("Paid game {GameId} created successfully", response.GameId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for paid game");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating paid game");
            return StatusCode(500, new { error = "Failed to create game", details = ex.Message });
        }
    }

    /// <summary>
    /// Join an existing paid game
    /// </summary>
    /// <param name="request">Game ID and wallet address</param>
    /// <returns>Game data with cells and image mapping</returns>
    [HttpPost("join")]
    [ProducesResponseType(typeof(CreateGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateGameResponse>> JoinGame([FromBody] JoinGameRequest request)
    {
        try
        {
            // Validate request
            if (request.GameId <= 0)
            {
                return BadRequest(new { error = "Invalid game ID" });
            }

            if (string.IsNullOrWhiteSpace(request.WalletAddress))
            {
                return BadRequest(new { error = "Wallet address is required" });
            }

            _logger.LogInformation(
                "User {WalletAddress} joining game {GameId}",
                request.WalletAddress, request.GameId);

            // Join the game
            var response = await _gameService.JoinPaidGame(request.GameId, request.WalletAddress);

            // Broadcast to all clients that the game was removed from lobby
            var lobby = await _gameService.GetLobbyGame(request.GameId);
            await _hubContext.Clients.All.SendAsync("ReceiveDeleteGame", lobby);

            _logger.LogInformation(
                "User {WalletAddress} joined game {GameId} successfully",
                request.WalletAddress, request.GameId);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Game {GameId} not found", request.GameId);
            return NotFound(new { error = "Game not found" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot join game {GameId}", request.GameId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game {GameId}", request.GameId);
            return StatusCode(500, new { error = "Failed to join game", details = ex.Message });
        }
    }

    /// <summary>
    /// Submit final score for a completed game
    /// </summary>
    /// <param name="request">Game ID, wallet address, score, and time elapsed</param>
    /// <returns>Success status</returns>
    [HttpPost("submit-score")]
    [ProducesResponseType(typeof(SubmitScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubmitScoreResponse>> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        try
        {
            // Validate request
            if (request.GameId <= 0)
            {
                return BadRequest(new { error = "Invalid game ID" });
            }

            if (string.IsNullOrWhiteSpace(request.WalletAddress))
            {
                return BadRequest(new { error = "Wallet address is required" });
            }

            if (request.Score < 0)
            {
                return BadRequest(new { error = "Score cannot be negative" });
            }

            _logger.LogInformation(
                "Submitting score {Score} for game {GameId} by wallet {WalletAddress}",
                request.Score, request.GameId, request.WalletAddress);

            // Submit the score
            var response = await _gameService.SubmitScore(
                request.GameId,
                request.WalletAddress,
                request.Score,
                request.TimeElapsed);

            _logger.LogInformation(
                "Score submitted successfully for game {GameId}. Winner: {Winner}",
                request.GameId, response.Winner ?? "TBD");

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Game {GameId} not found", request.GameId);
            return NotFound(new { error = "Game not found" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot submit score for game {GameId}", request.GameId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting score for game {GameId}", request.GameId);
            return StatusCode(500, new { error = "Failed to submit score", details = ex.Message });
        }
    }

    /// <summary>
    /// Get game status and details
    /// </summary>
    /// <param name="gameId">Game ID</param>
    /// <returns>Game status information</returns>
    [HttpGet("{gameId}")]
    [ProducesResponseType(typeof(GameStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameStatusResponse>> GetGameStatus(long gameId)
    {
        try
        {
            var status = await _gameService.GetGameStatus(gameId);

            if (status == null)
            {
                return NotFound(new { error = "Game not found" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game status for {GameId}", gameId);
            return StatusCode(500, new { error = "Failed to get game status", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a game (only owner can cancel if no opponent has joined)
    /// </summary>
    /// <param name="gameId">Game ID</param>
    /// <param name="walletAddress">Wallet address of the owner</param>
    /// <returns>Success status</returns>
    [HttpDelete("{gameId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelGame(long gameId, [FromQuery] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { error = "Wallet address is required" });
            }

            _logger.LogInformation(
                "Cancelling game {GameId} by wallet {WalletAddress}",
                gameId, walletAddress);

            await _gameService.CancelGame(gameId, walletAddress);

            // Broadcast that game was removed
            var lobby = await _gameService.GetLobbyGame(gameId);
            await _hubContext.Clients.All.SendAsync("ReceiveDeleteGame", lobby);

            _logger.LogInformation("Game {GameId} cancelled successfully", gameId);

            return Ok(new { message = "Game cancelled successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Game {GameId} not found", gameId);
            return NotFound(new { error = "Game not found" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized cancellation attempt for game {GameId}", gameId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel game {GameId}", gameId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling game {GameId}", gameId);
            return StatusCode(500, new { error = "Failed to cancel game", details = ex.Message });
        }
    }
}