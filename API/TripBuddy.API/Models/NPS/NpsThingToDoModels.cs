namespace TripBuddy.API.Models.NPS;

/// <summary>
/// Model for NPS things to do data format
/// </summary>
public class NpsThingToDoData
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public string? Duration { get; set; }
    public List<string>? Tags { get; set; }
    public string? SeasonDescription { get; set; }
    public List<NpsThingToDoImage>? Images { get; set; }
    public string? AccessibilityInformation { get; set; }
    public List<NpsThingToDoFee>? Fees { get; set; }
    public string? Location { get; set; }
    public List<NpsRelatedParkInfo>? RelatedParks { get; set; }
}

public class NpsThingToDoImage
{
    public string Url { get; set; } = string.Empty;
    public string Credit { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class NpsThingToDoFee
{
    public string Cost { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class NpsRelatedParkInfo
{
    public string ParkCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}