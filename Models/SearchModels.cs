using System.ComponentModel.DataAnnotations;

namespace TripBuddy.Models
{
    public class SearchRequest
    {
        [Required]
        public string Query { get; set; } = string.Empty;

        public int? Limit { get; set; } = 10;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double? RadiusKm { get; set; }
    }

    public class SearchResponse
    {
        public string Query { get; set; } = string.Empty;

        public List<ParkResult> Results { get; set; } = new();

        public string ContextualResponse { get; set; } = string.Empty;

        public int TotalResults { get; set; }

        public double ProcessingTimeMs { get; set; }
    }

    public class ParkResult
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string ParkType { get; set; } = string.Empty;

        public List<string> Features { get; set; } = new();

        public List<string> Activities { get; set; } = new();

        public double Similarity { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ParkData
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public List<string> Features { get; set; } = new();

        public List<string> Activities { get; set; } = new();
    }
}