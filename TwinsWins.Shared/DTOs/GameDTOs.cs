namespace TwinsWins.Shared.DTOs;

/// <summary>
/// Request to create a new paid game
/// </summary>
public record CreateGameRequest
{
    public string WalletAddress { get; init; } = string.Empty;
    public decimal Stake { get; init; }
}

/// <summary>
/// Response containing game data
/// </summary>
public record CreateGameResponse
{
    public long GameId { get; init; }
    public List<CellDto> Cells { get; init; } = new();
    public Dictionary<int, int> ImageIdMap { get; init; } = new();
}

/// <summary>
/// Request to join an existing game
/// </summary>
public record JoinGameRequest
{
    public long GameId { get; init; }
    public string WalletAddress { get; init; } = string.Empty;
}

/// <summary>
/// DTO for cell data transfer
/// </summary>
public record CellDto
{
    public int Id { get; init; }
    public string ImagePath { get; init; } = string.Empty;
}

/// <summary>
/// Request to submit final score
/// </summary>
public record SubmitScoreRequest
{
    public long GameId { get; init; }
    public string WalletAddress { get; init; } = string.Empty;
    public int Score { get; init; }
    public int TimeElapsed { get; init; }
}

/// <summary>
/// Response for lobby statistics
/// </summary>
public record LobbyStatsResponse
{
    public int TotalGames { get; init; }
    public decimal TotalStaked { get; init; }
    public decimal AverageStake { get; init; }
    public decimal MinStake { get; init; }
    public decimal MaxStake { get; init; }
    public int GamesLast24Hours { get; init; }
}