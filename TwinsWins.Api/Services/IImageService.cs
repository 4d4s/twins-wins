namespace TwinsWins.Api.Services;

/// <summary>
/// Service interface for managing game images
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Gets random image pairs for the game
    /// </summary>
    /// <param name="pairCount">Number of pairs needed (default 9)</param>
    /// <returns>List of image pairs</returns>
    List<ImagePair> GetRandomImagePairs(int pairCount = 9);

    /// <summary>
    /// Gets all available images
    /// </summary>
    /// <returns>List of all image paths</returns>
    List<string> GetAllImages();
}