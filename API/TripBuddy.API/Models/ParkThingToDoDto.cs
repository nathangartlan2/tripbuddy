using System.Text.Json.Serialization;

namespace TripBuddy.API.Models;

/// <summary>
/// API response model for park activities and things to do
/// </summary>
public class ParkThingToDoDto
{
    public int Id { get; set; }
    public int ParkId { get; set; }

    // NPS identifier
    public string? NpsThingId { get; set; }

    // Core activity information
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }

    // Activity details
    public string? Duration { get; set; }
    public string? Season { get; set; }
    public string[]? ActivityTags { get; set; }

    // Practical information
    public string? AccessibilityInfo { get; set; }
    public string? FeeInfo { get; set; }
    public string? LocationDescription { get; set; }
    public string? Url { get; set; }

    // Computed properties
    public bool HasFee => !string.IsNullOrEmpty(FeeInfo) &&
                          !FeeInfo.ToLowerInvariant().Contains("free") &&
                          !FeeInfo.ToLowerInvariant().Contains("no fee");

    public bool IsAccessible => !string.IsNullOrEmpty(AccessibilityInfo);

    public bool IsYearRound => Season?.ToLowerInvariant().Contains("year round") == true;

    public string? PrimaryActivityTag => ActivityTags?.FirstOrDefault();

    // Metadata
    public bool IsActive { get; set; } = true;

    // Optional parent park info (for standalone activity responses)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ParkSummaryDto? Park { get; set; }
}

/// <summary>
/// Lightweight park information for inclusion in activity responses
/// </summary>
public class ParkSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NpsParkCode { get; set; }
    public string? Location { get; set; }
    public string? StateCode { get; set; }
}