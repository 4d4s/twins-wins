namespace TwinsWins.Shared.Models
{
    /// <summary>
    /// Represents a single cell (card) in the memory game.
    /// Using 'record' for immutability - perfect for Fluxor state management.
    /// </summary>
    public record Cell
    {
        /// <summary>
        /// Unique identifier for this cell (1-18 for 9 pairs)
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Path to the image displayed on this cell
        /// Example: "/images/game/cat/raw/cat1.jpg"
        /// </summary>
        public string ImagePath { get; init; } = string.Empty;

        /// <summary>
        /// Whether this cell has been successfully matched with its pair
        /// </summary>
        public bool IsMatched { get; init; }

        /// <summary>
        /// Whether this cell is currently face-up (revealed)
        /// </summary>
        public bool IsRevealed { get; init; }

        /// <summary>
        /// Whether this cell is currently playing an animation
        /// </summary>
        public bool IsAnimating { get; init; }

        /// <summary>
        /// Whether this cell can be clicked
        /// </summary>
        public bool IsClickable => !IsMatched && !IsRevealed && !IsAnimating;
    }
}
