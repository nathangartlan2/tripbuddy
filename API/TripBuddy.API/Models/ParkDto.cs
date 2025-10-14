using System.Text.Json.Serialization;

namespace TripBuddy.API.Models;

/// <summary>
/// API response model for park information
/// </summary>
public class ParkDto : ISearchResponsePreview
{
    public int Id { get; set; }

    // Core park information
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? FullDescription { get; set; }

    // NPS-specific identifiers
    public string? NpsParkCode { get; set; }
    public string? NpsDesignation { get; set; }

    // Location information
    public string? Location { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? StateCode { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Contact information
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WebsiteUrl { get; set; }

    // Legacy fields
    public string? ParkType { get; set; }

    // Computed properties
    public string? FullAddress => BuildFullAddress();
    public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;

    // Metadata (excluding internal timestamps)
    public bool IsActive { get; set; } = true;

    // ISearchResponsePreview implementation
    [JsonIgnore]
    public SearchResultType Type => SearchResultType.Park;

    [JsonIgnore]
    public string Title => Name;

    [JsonIgnore]
    public string PreviewText => Description ?? FullDescription ?? "National park destination";

    // Related data (optional, can be included via query parameters)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ParkThingToDoDto>? ThingsToDo { get; set; }

    private string? BuildFullAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(StreetAddress))
            parts.Add(StreetAddress);

        if (!string.IsNullOrEmpty(City))
            parts.Add(City);

        if (!string.IsNullOrEmpty(StateCode))
            parts.Add(StateCode);

        if (!string.IsNullOrEmpty(PostalCode))
            parts.Add(PostalCode);

        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }
}