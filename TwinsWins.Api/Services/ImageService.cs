namespace TwinsWins.Api.Services;

/// <summary>
/// Service for managing game images from the file system
/// </summary>
public class ImageService : IImageService
{
    private readonly string _imageDirectory;
    private readonly ILogger<ImageService>? _logger;
    private List<string>? _cachedImages;

    public ImageService(string imageDirectory, ILogger<ImageService>? logger = null)
    {
        _imageDirectory = imageDirectory;
        _logger = logger;
    }

    public List<ImagePair> GetRandomImagePairs(int pairCount = 9)
    {
        var allImages = GetAllImages();

        if (allImages.Count < pairCount * 2)
        {
            throw new InvalidOperationException(
                $"Not enough images available. Need {pairCount * 2}, but only have {allImages.Count}");
        }

        // Group images by category
        var imagesByCategory = allImages
            .GroupBy(img => GetCategoryFromPath(img))
            .Where(g => g.Count() >= 2) // Only categories with at least 2 images
            .ToList();

        if (imagesByCategory.Count < pairCount)
        {
            throw new InvalidOperationException(
                $"Not enough image categories. Need {pairCount}, but only have {imagesByCategory.Count}");
        }

        var random = new Random();
        var pairs = new List<ImagePair>();

        // Select random categories
        var selectedCategories = imagesByCategory
            .OrderBy(_ => random.Next())
            .Take(pairCount)
            .ToList();

        foreach (var category in selectedCategories)
        {
            // Get 2 random images from this category
            var categoryImages = category.OrderBy(_ => random.Next()).Take(2).ToList();

            pairs.Add(new ImagePair
            {
                ImagePath1 = categoryImages[0],
                ImagePath2 = categoryImages[1]
            });
        }

        return pairs;
    }

    public List<string> GetAllImages()
    {
        if (_cachedImages != null)
        {
            return _cachedImages;
        }

        try
        {
            if (!Directory.Exists(_imageDirectory))
            {
                _logger?.LogWarning("Image directory does not exist: {Directory}", _imageDirectory);
                return new List<string>();
            }

            // Get all image files recursively
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var images = Directory
                .GetFiles(_imageDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Select(f => ConvertToRelativePath(f))
                .ToList();

            _cachedImages = images;
            _logger?.LogInformation("Loaded {Count} images from {Directory}", images.Count, _imageDirectory);

            return images;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading images from {Directory}", _imageDirectory);
            return new List<string>();
        }
    }

    private string ConvertToRelativePath(string fullPath)
    {
        // Convert physical path to web path
        // e.g., "wwwroot/images/game/cat/raw/cat1.jpg" -> "/images/game/cat/raw/cat1.jpg"
        var relativePath = fullPath.Replace(_imageDirectory, "")
                                   .Replace("\\", "/")
                                   .TrimStart('/');

        return $"/{_imageDirectory.Replace("wwwroot/", "").Replace("wwwroot\\", "")}/{relativePath}";
    }

    private string GetCategoryFromPath(string imagePath)
    {
        // Extract category from path
        // e.g., "/images/game/cat/raw/cat1.jpg" -> "cat"
        var parts = imagePath.Split('/', '\\');
        if (parts.Length > 3)
        {
            return parts[3]; // Assuming structure: /images/game/{category}/...
        }
        return "default";
    }
}