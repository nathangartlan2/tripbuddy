namespace TripBuddy.API.Models.NPS;

/// <summary>
/// Model for deserializing NPS API park data
/// </summary>
public class NpsParkData
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ParkCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string LatLong { get; set; } = string.Empty;
    public string DirectionsInfo { get; set; } = string.Empty;
    public string DirectionsUrl { get; set; } = string.Empty;
    public List<NpsOperatingHours> OperatingHours { get; set; } = new List<NpsOperatingHours>();
    public List<NpsAddress> Addresses { get; set; } = new List<NpsAddress>();
}

public class NpsOperatingHours
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public NpsStandardHours StandardHours { get; set; } = new NpsStandardHours();
}

public class NpsStandardHours
{
    public string Monday { get; set; } = string.Empty;
    public string Tuesday { get; set; } = string.Empty;
    public string Wednesday { get; set; } = string.Empty;
    public string Thursday { get; set; } = string.Empty;
    public string Friday { get; set; } = string.Empty;
    public string Saturday { get; set; } = string.Empty;
    public string Sunday { get; set; } = string.Empty;
}

public class NpsAddress
{
    public string Type { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public string Line3 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}