using Microsoft.AspNetCore.Mvc;
using TwinsWins.Shared.Models;
using TwinsWins.Api.Services;

namespace TwinsWins.Api.Controllers;

/// <summary>
/// Handles lobby operations - viewing and managing available games
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LobbyController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<LobbyController> _logger;

    public LobbyController(
        IGameService gameService,
        ILogger<LobbyController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available games in the lobby
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>List of available games</returns>
    [HttpGet("games")]
    [ProducesResponseType(typeof(List<GameLobby>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<GameLobby>>> GetAvailableGames(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                pageSize = 20;
            }

            _logger.LogInformation("Fetching available games (page: {Page}, size: {PageSize})", page, pageSize);

            var games = await _gameService.GetAvailableGames(page, pageSize);

            _logger.LogInformation("Returned {Count} available games", games.Count);

            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available games");
            return StatusCode(500, new { error = "Failed to fetch games", details = ex.Message });
        }
    }

    /// <summary>
    /// Get lobby statistics
    /// </summary>
    /// <returns>Lobby stats (total games, total stake, etc.)</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(LobbyStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LobbyStatsResponse>> GetLobbyStats()
    {
        try
        {
            _logger.LogInformation("Fetching lobby statistics");

            var stats = await _gameService.GetLobbyStats();

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lobby stats");
            return StatusCode(500, new { error = "Failed to fetch stats", details = ex.Message });
        }
    }

    /// <summary>
    /// Get games created by a specific wallet
    /// </summary>
    /// <param name="walletAddress">Wallet address</param>
    /// <returns>List of games created by this wallet</returns>
    [HttpGet("games/by-owner/{walletAddress}")]
    [ProducesResponseType(typeof(List<GameLobby>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<GameLobby>>> GetGamesByOwner(string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { error = "Wallet address is required" });
            }

            _logger.LogInformation("Fetching games for owner {WalletAddress}", walletAddress);

            var games = await _gameService.GetGamesByOwner(walletAddress);

            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching games for owner {WalletAddress}", walletAddress);
            return StatusCode(500, new { error = "Failed to fetch games", details = ex.Message });
        }
    }

    /// <summary>
    /// Search games by stake range
    /// </summary>
    /// <param name="minStake">Minimum stake</param>
    /// <param name="maxStake">Maximum stake</param>
    /// <returns>List of games within stake range</returns>
    [HttpGet("games/by-stake")]
    [ProducesResponseType(typeof(List<GameLobby>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<GameLobby>>> GetGamesByStake(
        [FromQuery] decimal minStake = 0,
        [FromQuery] decimal? maxStake = null)
    {
        try
        {
            if (minStake < 0)
            {
                return BadRequest(new { error = "Minimum stake cannot be negative" });
            }

            if (maxStake.HasValue && maxStake.Value < minStake)
            {
                return BadRequest(new { error = "Maximum stake must be greater than minimum stake" });
            }

            _logger.LogInformation(
                "Fetching games by stake range: {MinStake} - {MaxStake}",
                minStake, maxStake);

            var games = await _gameService.GetGamesByStakeRange(minStake, maxStake);

            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching games by stake range");
            return StatusCode(500, new { error = "Failed to fetch games", details = ex.Message });
        }
    }
}

/// <summary>
/// Lobby statistics response
/// </summary>
public class LobbyStatsResponse
{
    public int TotalGames { get; set; }
    public decimal TotalStaked { get; set; }
    public decimal AverageStake { get; set; }
    public decimal MinStake { get; set; }
    public decimal MaxStake { get; set; }
    public int GamesLast24Hours { get; set; }
}