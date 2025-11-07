using TwinsWins.Shared.DTOs;
using TwinsWins.Shared.Models;
using TwinsWins.Api.Data.Repositories;
using TwinsWins.Api.Data.Models;
using System.Text.Json;

namespace TwinsWins.Api.Services;

/// <summary>
/// Implementation of game service with business logic
/// </summary>
public class GameService : IGameService
{
    private readonly IGameLobbyRepository _lobbyRepository;
    private readonly IGameTransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IImageService _imageService;
    private readonly ITonWalletService _tonWalletService;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IGameLobbyRepository lobbyRepository,
        IGameTransactionRepository transactionRepository,
        IUserRepository userRepository,
        IImageService imageService,
        ITonWalletService tonWalletService,
        ILogger<GameService> logger)
    {
        _lobbyRepository = lobbyRepository;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _imageService = imageService;
        _tonWalletService = tonWalletService;
        _logger = logger;
    }

    public Task<CreateGameResponse> CreateFreeGame()
    {
        _logger.LogInformation("Creating free practice game");

        // Get random image pairs
        var imagePairs = _imageService.GetRandomImagePairs(9);

        // Create cells and image map
        var (cells, imageIdMap) = CreateGameCells(imagePairs);

        var response = new CreateGameResponse
        {
            GameId = 0, // Free game has no ID
            Cells = cells,
            ImageIdMap = imageIdMap
        };

        return Task.FromResult(response);
    }

    public async Task<CreateGameResponse> CreatePaidGame(string walletAddress, decimal stake)
    {
        _logger.LogInformation("Creating paid game for {WalletAddress} with stake {Stake}", walletAddress, stake);

        // Validate stake
        if (stake <= 0)
        {
            throw new ArgumentException("Stake must be greater than 0", nameof(stake));
        }

        // Get or create user
        var user = await _userRepository.GetByWalletAddressAsync(walletAddress);
        if (user == null)
        {
            user = new User
            {
                WalletAddress = walletAddress,
                Username = $"Player_{walletAddress[..8]}",
                Created = DateTime.UtcNow
            };
            user = await _userRepository.CreateAsync(user);
        }

        // Get random image pairs
        var imagePairs = _imageService.GetRandomImagePairs(9);

        // Create cells and image map
        var (cells, imageIdMap) = CreateGameCells(imagePairs);

        // Create game body (to store in database)
        var gameBody = new GameBody
        {
            Cells = cells,
            ImageIdMap = imageIdMap
        };

        // Create lobby entry
        var lobbyGame = new GameLobbyEntity
        {
            OwnerId = user.Id,
            Stake = stake,
            Created = DateTime.UtcNow,
            Body = JsonSerializer.Serialize(gameBody)
        };

        lobbyGame = await _lobbyRepository.CreateAsync(lobbyGame);

        // Create transaction record
        var transaction = new GameTransactionEntity
        {
            Id = lobbyGame.Id,
            OwnerId = user.Id,
            Stake = stake,
            Created = DateTime.UtcNow
        };

        await _transactionRepository.CreateAsync(transaction);

        _logger.LogInformation("Paid game {GameId} created successfully", lobbyGame.Id);

        return new CreateGameResponse
        {
            GameId = lobbyGame.Id,
            Cells = cells,
            ImageIdMap = imageIdMap
        };
    }

    public async Task<CreateGameResponse> JoinPaidGame(long gameId, string walletAddress)
    {
        _logger.LogInformation("User {WalletAddress} joining game {GameId}", walletAddress, gameId);

        // Get game from lobby
        var lobbyGame = await _lobbyRepository.GetByIdAsync(gameId);
        if (lobbyGame == null)
        {
            throw new KeyNotFoundException($"Game {gameId} not found");
        }

        // Get or create user
        var user = await _userRepository.GetByWalletAddressAsync(walletAddress);
        if (user == null)
        {
            user = new User
            {
                WalletAddress = walletAddress,
                Username = $"Player_{walletAddress[..8]}",
                Created = DateTime.UtcNow
            };
            user = await _userRepository.CreateAsync(user);
        }

        // Check if user is trying to join their own game
        if (lobbyGame.OwnerId == user.Id)
        {
            throw new InvalidOperationException("Cannot join your own game");
        }

        // Deserialize game body
        var gameBody = JsonSerializer.Deserialize<GameBody>(lobbyGame.Body);
        if (gameBody == null)
        {
            throw new InvalidOperationException("Invalid game data");
        }

        // Update transaction with opponent
        await _transactionRepository.SetOpponentAsync(gameId, user.Id);

        // Remove from lobby
        await _lobbyRepository.DeleteAsync(gameId);

        _logger.LogInformation("User {WalletAddress} joined game {GameId} successfully", walletAddress, gameId);

        return new CreateGameResponse
        {
            GameId = gameId,
            Cells = gameBody.Cells,
            ImageIdMap = gameBody.ImageIdMap
        };
    }

    public async Task<SubmitScoreResponse> SubmitScore(long gameId, string walletAddress, int score, int timeElapsed)
    {
        _logger.LogInformation(
            "Submitting score {Score} for game {GameId} by {WalletAddress}",
            score, gameId, walletAddress);

        // Get transaction
        var transaction = await _transactionRepository.GetByIdAsync(gameId);
        if (transaction == null)
        {
            throw new KeyNotFoundException($"Game {gameId} not found");
        }

        // Get user
        var user = await _userRepository.GetByWalletAddressAsync(walletAddress);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Determine if user is owner or opponent
        bool isOwner = transaction.OwnerId == user.Id;
        bool isOpponent = transaction.OpponentId == user.Id;

        if (!isOwner && !isOpponent)
        {
            throw new InvalidOperationException("User is not part of this game");
        }

        // Update score
        if (isOwner)
        {
            if (transaction.OwnerScore.HasValue)
            {
                throw new InvalidOperationException("Score already submitted");
            }
            transaction.OwnerScore = score;
        }
        else
        {
            if (transaction.OpponentScore.HasValue)
            {
                throw new InvalidOperationException("Score already submitted");
            }
            transaction.OpponentScore = score;
        }

        await _transactionRepository.UpdateAsync(transaction);

        // Check if both players have submitted scores
        var response = new SubmitScoreResponse
        {
            IsGameComplete = transaction.OwnerScore.HasValue && transaction.OpponentScore.HasValue
        };

        if (response.IsGameComplete)
        {
            // Determine winner
            var ownerScore = transaction.OwnerScore!.Value;
            var opponentScore = transaction.OpponentScore!.Value;

            var owner = await _userRepository.GetByIdAsync(transaction.OwnerId);
            var opponent = await _userRepository.GetByIdAsync(transaction.OpponentId!.Value);

            if (ownerScore > opponentScore)
            {
                response.Winner = owner?.WalletAddress;
                response.WinnerScore = ownerScore;
                response.LoserScore = opponentScore;
            }
            else if (opponentScore > ownerScore)
            {
                response.Winner = opponent?.WalletAddress;
                response.WinnerScore = opponentScore;
                response.LoserScore = ownerScore;
            }
            else
            {
                response.Winner = "Draw";
                response.WinnerScore = ownerScore;
                response.LoserScore = opponentScore;
            }

            // Calculate prize (2x stake minus fee)
            var totalStake = transaction.Stake * 2;
            var developerFee = totalStake * 0.05m; // 5% fee
            response.PrizeAmount = totalStake - developerFee;

            // TODO: Process TON blockchain transaction
            // response.TransactionHash = await _tonWalletService.SendPrizeAsync(response.Winner, response.PrizeAmount);

            _logger.LogInformation(
                "Game {GameId} completed. Winner: {Winner}, Prize: {Prize} TON",
                gameId, response.Winner, response.PrizeAmount);
        }

        return response;
    }

    public async Task<GameStatusResponse> GetGameStatus(long gameId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(gameId);
        if (transaction == null)
        {
            throw new KeyNotFoundException($"Game {gameId} not found");
        }

        var owner = await _userRepository.GetByIdAsync(transaction.OwnerId);
        var opponent = transaction.OpponentId.HasValue
            ? await _userRepository.GetByIdAsync(transaction.OpponentId.Value)
            : null;

        var status = GameStatus.WaitingForOpponent;
        if (opponent != null)
        {
            if (transaction.OwnerScore.HasValue && transaction.OpponentScore.HasValue)
            {
                status = GameStatus.BothCompleted;
            }
            else if (transaction.OwnerScore.HasValue)
            {
                status = GameStatus.OpponentPlaying;
            }
            else if (transaction.OpponentScore.HasValue)
            {
                status = GameStatus.OwnerPlaying;
            }
        }

        string? winner = null;
        if (status == GameStatus.BothCompleted)
        {
            if (transaction.OwnerScore > transaction.OpponentScore)
            {
                winner = owner?.WalletAddress;
            }
            else if (transaction.OpponentScore > transaction.OwnerScore)
            {
                winner = opponent?.WalletAddress;
            }
            else
            {
                winner = "Draw";
            }
        }

        return new GameStatusResponse
        {
            GameId = gameId,
            OwnerAddress = owner?.WalletAddress ?? string.Empty,
            OpponentAddress = opponent?.WalletAddress,
            OwnerScore = transaction.OwnerScore,
            OpponentScore = transaction.OpponentScore,
            Stake = transaction.Stake,
            Created = transaction.Created,
            Status = status,
            Winner = winner
        };
    }

    public async Task<GameLobby> GetLobbyGame(long gameId)
    {
        var lobbyGame = await _lobbyRepository.GetByIdAsync(gameId);
        if (lobbyGame == null)
        {
            throw new KeyNotFoundException($"Game {gameId} not found");
        }

        var owner = await _userRepository.GetByIdAsync(lobbyGame.OwnerId);

        return new GameLobby
        {
            Id = lobbyGame.Id,
            OwnerId = lobbyGame.OwnerId,
            OwnerAddress = owner?.WalletAddress ?? string.Empty,
            OwnerUsername = owner?.Username ?? "Unknown",
            Stake = lobbyGame.Stake,
            Created = lobbyGame.Created
        };
    }

    public async Task CancelGame(long gameId, string walletAddress)
    {
        var lobbyGame = await _lobbyRepository.GetByIdAsync(gameId);
        if (lobbyGame == null)
        {
            throw new KeyNotFoundException($"Game {gameId} not found");
        }

        var user = await _userRepository.GetByWalletAddressAsync(walletAddress);
        if (user == null || user.Id != lobbyGame.OwnerId)
        {
            throw new UnauthorizedAccessException("Only the game owner can cancel the game");
        }

        // Check if game has been joined
        var transaction = await _transactionRepository.GetByIdAsync(gameId);
        if (transaction?.OpponentId != null)
        {
            throw new InvalidOperationException("Cannot cancel a game that has been joined");
        }

        await _lobbyRepository.DeleteAsync(gameId);
        await _transactionRepository.DeleteAsync(gameId);

        _logger.LogInformation("Game {GameId} cancelled by owner", gameId);
    }

    public async Task<List<GameLobby>> GetAvailableGames(int page = 1, int pageSize = 20)
    {
        var lobbyGames = await _lobbyRepository.GetAllAsync(page, pageSize);

        var result = new List<GameLobby>();
        foreach (var lobbyGame in lobbyGames)
        {
            var owner = await _userRepository.GetByIdAsync(lobbyGame.OwnerId);
            result.Add(new GameLobby
            {
                Id = lobbyGame.Id,
                OwnerId = lobbyGame.OwnerId,
                OwnerAddress = owner?.WalletAddress ?? string.Empty,
                OwnerUsername = owner?.Username ?? "Unknown",
                Stake = lobbyGame.Stake,
                Created = lobbyGame.Created
            });
        }

        return result;
    }

    public async Task<List<GameLobby>> GetGamesByOwner(string walletAddress)
    {
        var user = await _userRepository.GetByWalletAddressAsync(walletAddress);
        if (user == null)
        {
            return new List<GameLobby>();
        }

        var lobbyGames = await _lobbyRepository.GetByOwnerIdAsync(user.Id);

        return lobbyGames.Select(lg => new GameLobby
        {
            Id = lg.Id,
            OwnerId = lg.OwnerId,
            OwnerAddress = user.WalletAddress,
            OwnerUsername = user.Username,
            Stake = lg.Stake,
            Created = lg.Created
        }).ToList();
    }

    public async Task<List<GameLobby>> GetGamesByStakeRange(decimal minStake, decimal? maxStake)
    {
        var lobbyGames = await _lobbyRepository.GetByStakeRangeAsync(minStake, maxStake);

        var result = new List<GameLobby>();
        foreach (var lobbyGame in lobbyGames)
        {
            var owner = await _userRepository.GetByIdAsync(lobbyGame.OwnerId);
            result.Add(new GameLobby
            {
                Id = lobbyGame.Id,
                OwnerId = lobbyGame.OwnerId,
                OwnerAddress = owner?.WalletAddress ?? string.Empty,
                OwnerUsername = owner?.Username ?? "Unknown",
                Stake = lobbyGame.Stake,
                Created = lobbyGame.Created
            });
        }

        return result;
    }

    public async Task<LobbyStatsResponse    > GetLobbyStats()
    {
        var stats = await _lobbyRepository.GetStatsAsync();

        return new LobbyStatsResponse
        {
            TotalGames = stats.TotalGames,
            TotalStaked = stats.TotalStaked,
            AverageStake = stats.TotalGames > 0 ? stats.TotalStaked / stats.TotalGames : 0,
            MinStake = stats.MinStake,
            MaxStake = stats.MaxStake,
            GamesLast24Hours = stats.GamesLast24Hours
        };
    }

    // Helper method to create game cells and image map
    private (List<CellDto> cells, Dictionary<int, int> imageIdMap) CreateGameCells(List<ImagePair> imagePairs)
    {
        var cells = new List<CellDto>();
        var imageIdMap = new Dictionary<int, int>();

        int id = 1;
        foreach (var imagePair in imagePairs)
        {
            var cell1 = new CellDto { Id = id++, ImagePath = imagePair.ImagePath1 };
            var cell2 = new CellDto { Id = id++, ImagePath = imagePair.ImagePath2 };

            cells.Add(cell1);
            cells.Add(cell2);

            // Map cells to each other (bidirectional)
            imageIdMap[cell1.Id] = cell2.Id;
            imageIdMap[cell2.Id] = cell1.Id;
        }

        // Shuffle cells
        var random = new Random();
        cells = cells.OrderBy(_ => random.Next()).ToList();

        return (cells, imageIdMap);
    }
}

/// <summary>
/// Game body stored in database
/// </summary>
public class GameBody
{
    public List<CellDto> Cells { get; set; } = new();
    public Dictionary<int, int> ImageIdMap { get; set; } = new();
}

/// <summary>
/// Image pair for game generation
/// </summary>
public class ImagePair
{
    public string ImagePath1 { get; set; } = string.Empty;
    public string ImagePath2 { get; set; } = string.Empty;
}