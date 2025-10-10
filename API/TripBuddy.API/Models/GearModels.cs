using System.Text.Json;

namespace TripBuddy.API.Models
{
    // Simplified core models for gear-focused trip planning

    public class TripContext
    {
        public string Destination { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty; // "1 day", "3 days", "1 week"
        public string ExperienceLevel { get; set; } = string.Empty;
        public int GroupSize { get; set; } = 1;
        public string TripType { get; set; } = "backpacking"; // backpacking, car camping, day hiking
    }

    public class GearItem
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "shelter", "cooking", "clothing", "safety"
        public bool IsRequired { get; set; } = true;
        public string? Note { get; set; } // AI reasoning for changes
        public GearModification? Modification { get; set; } // null = base item, otherwise AI change
    }

    public class GearModification
    {
        public string Action { get; set; } = string.Empty; // "added", "removed", "upgraded"
        public string Reason { get; set; } = string.Empty; // Brief AI explanation
        public string? ReplacedItem { get; set; } // If upgrading/replacing
    }

    public class GearList
    {
        public List<GearItem> Items { get; set; } = new();
        public TripContext Context { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    // API Request/Response models
    public class GenerateGearListRequest
    {
        public TripContext TripContext { get; set; } = new();
        public List<string>? BaseGearCategories { get; set; } // Optional: limit to specific categories
    }

    public class GenerateGearListResponse
    {
        public GearList GearList { get; set; } = new();
        public string Summary { get; set; } = string.Empty; // Brief AI explanation of key changes
        public List<string> Warnings { get; set; } = new(); // Important safety/logistics notes
    }

    // Base gear templates
    public class BaseGearTemplate
    {
        public string TripType { get; set; } = string.Empty;
        public List<GearCategory> Categories { get; set; } = new();
    }

    public class GearCategory
    {
        public string Name { get; set; } = string.Empty; // "Shelter", "Cooking", "Clothing"
        public List<string> BaseItems { get; set; } = new(); // Default items for this category
        public bool IsEssential { get; set; } = true; // Can this category be skipped for some trips?
    }
}