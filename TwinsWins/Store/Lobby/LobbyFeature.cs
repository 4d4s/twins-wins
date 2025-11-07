using Fluxor;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Lobby;

/// <summary>
/// Feature registration for Lobby state
/// This tells Fluxor about the Lobby state slice
/// </summary>
public class LobbyFeature : Feature<LobbyState>
{
    public override string GetName() => "Lobby";

    protected override LobbyState GetInitialState() => new LobbyState
    {
        AvailableGames = new List<GameLobby>(),
        Stats = null,
        IsLoading = false,
        ErrorMessage = null,
        CurrentPage = 1,
        PageSize = 12,
        MinStakeFilter = null,
        MaxStakeFilter = null,
        SortBy = "Stake",
        SortAscending = false,
        IsAutoRefreshing = false
    };
}