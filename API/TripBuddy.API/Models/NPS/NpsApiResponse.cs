namespace TripBuddy.API.Models.NPS;

/// <summary>
/// Wrapper class for NPS API response format
/// </summary>
public class NpsApiResponse<T>
{
    public string? Total { get; set; }
    public string? Limit { get; set; }
    public string? Start { get; set; }
    public List<T> Data { get; set; } = new List<T>();
}