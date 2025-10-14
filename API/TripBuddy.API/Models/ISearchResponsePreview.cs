namespace TripBuddy.API.Models
{
    /// <summary>
    /// Interface for providing preview information for search results
    /// </summary>
    public interface ISearchResponsePreview
    {
        /// <summary>
        /// The type of the underlying data object
        /// </summary>
        SearchResultType Type { get; }

        /// <summary>
        /// The ID of the underlying object for API fetching
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Display title for the preview
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Brief preview text or description
        /// </summary>
        string PreviewText { get; }
    }

    /// <summary>
    /// Enum denoting the type of backing data object
    /// </summary>
    public enum SearchResultType
    {
        Park,
        ParkThingToDo,
        GearItem,
        GearTemplate
    }
}