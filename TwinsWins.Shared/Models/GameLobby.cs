namespace TwinsWins.Shared.Models;

/// <summary>
/// Represents a game in the lobby waiting for an opponent.
/// Using 'record' for immutability - stored in LobbyState.
/// </summary>
public record GameLobby
{
    public long Id { get; init; }
    public long OwnerId { get; init; }
    public string OwnerAddress { get; init; } = string.Empty;
    public string OwnerUsername { get; init; } = string.Empty;
    public decimal Stake { get; init; }
    public DateTime Created { get; init; }
    public int? OwnerScore { get; init; }

    // Computed property for display
    public string TimeAgo => GetTimeAgo(Created);

    private static string GetTimeAgo(DateTime created)
    {
        var timeSpan = DateTime.UtcNow - created;
        if (timeSpan.TotalMinutes < 1) return "Just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} min ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hr ago";
        return created.ToShortDateString();
    }
}