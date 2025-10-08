using System.Text.Json;

namespace TripBuddy.API.Data
{
    public class Park
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
        public double[]? Embedding { get; set; } // Pre-computed or calculated embedding
    }

    public class ParksData
    {
        public static List<Park> GetParks()
        {
            return new List<Park>
            {
                new Park
                {
                    Id = "1",
                    Name = "Yosemite National Park",
                    Description = "Famous for its waterfalls, deep valleys, grand meadows, ancient giant sequoias, and vast wilderness area. Home to iconic landmarks like Half Dome and El Capitan.",
                    Location = "California, USA",
                    Latitude = 37.8651,
                    Longitude = -119.5383,
                    ParkType = "National Park",
                    Features = new List<string> { "waterfalls", "granite cliffs", "sequoia trees", "valleys", "meadows" },
                    Activities = new List<string> { "hiking", "rock climbing", "camping", "photography", "wildlife viewing" }
                },
                new Park
                {
                    Id = "2",
                    Name = "Grand Canyon National Park",
                    Description = "One of the world's most spectacular natural wonders, featuring a massive canyon carved by the Colorado River with layered red rock formations.",
                    Location = "Arizona, USA",
                    Latitude = 36.1069,
                    Longitude = -112.1129,
                    ParkType = "National Park",
                    Features = new List<string> { "canyon", "red rocks", "river", "geological formations", "desert" },
                    Activities = new List<string> { "hiking", "rafting", "scenic drives", "helicopter tours", "stargazing" }
                },
                new Park
                {
                    Id = "3",
                    Name = "Yellowstone National Park",
                    Description = "America's first national park, known for its geothermal features including Old Faithful geyser, hot springs, and diverse wildlife including bears and wolves.",
                    Location = "Wyoming, Montana, Idaho, USA",
                    Latitude = 44.4280,
                    Longitude = -110.5885,
                    ParkType = "National Park",
                    Features = new List<string> { "geysers", "hot springs", "wildlife", "mountains", "forests" },
                    Activities = new List<string> { "wildlife watching", "hiking", "camping", "fishing", "thermal viewing" }
                },
                new Park
                {
                    Id = "4",
                    Name = "Banff National Park",
                    Description = "Canada's oldest national park featuring stunning mountain landscapes, turquoise lakes, glaciers, and abundant wildlife in the Canadian Rockies.",
                    Location = "Alberta, Canada",
                    Latitude = 51.4968,
                    Longitude = -115.9281,
                    ParkType = "National Park",
                    Features = new List<string> { "mountains", "lakes", "glaciers", "forests", "alpine meadows" },
                    Activities = new List<string> { "hiking", "skiing", "canoeing", "mountaineering", "wildlife photography" }
                },
                new Park
                {
                    Id = "5",
                    Name = "Great Smoky Mountains National Park",
                    Description = "Known for its diversity of plant and animal life, ancient mountains, and remnants of Southern Appalachian mountain culture.",
                    Location = "Tennessee, North Carolina, USA",
                    Latitude = 35.6118,
                    Longitude = -83.4895,
                    ParkType = "National Park",
                    Features = new List<string> { "mountains", "forests", "waterfalls", "historic cabins", "wildflowers" },
                    Activities = new List<string> { "hiking", "camping", "fishing", "historic tours", "nature walks" }
                },
                new Park
                {
                    Id = "6",
                    Name = "Zion National Park",
                    Description = "Features massive sandstone cliffs of cream, pink, and red that tower into brilliant blue sky, along with slot canyons and desert landscapes.",
                    Location = "Utah, USA",
                    Latitude = 37.2982,
                    Longitude = -113.0263,
                    ParkType = "National Park",
                    Features = new List<string> { "sandstone cliffs", "slot canyons", "desert", "rivers", "mesas" },
                    Activities = new List<string> { "hiking", "canyoneering", "rock climbing", "camping", "river walking" }
                },
                new Park
                {
                    Id = "7",
                    Name = "Acadia National Park",
                    Description = "Coastal park featuring rugged coastline, granite peaks, woodlands, and lakes. Known for its stunning sunrise views and diverse ecosystems.",
                    Location = "Maine, USA",
                    Latitude = 44.3386,
                    Longitude = -68.2733,
                    ParkType = "National Park",
                    Features = new List<string> { "coastline", "granite peaks", "forests", "lakes", "islands" },
                    Activities = new List<string> { "hiking", "biking", "kayaking", "bird watching", "sunrise viewing" }
                },
                new Park
                {
                    Id = "8",
                    Name = "Rocky Mountain National Park",
                    Description = "High-altitude wilderness featuring majestic mountain views, alpine lakes, and diverse wildlife including elk, bighorn sheep, and black bears.",
                    Location = "Colorado, USA",
                    Latitude = 40.3428,
                    Longitude = -105.6836,
                    ParkType = "National Park",
                    Features = new List<string> { "high altitude", "alpine lakes", "mountains", "tundra", "wildlife" },
                    Activities = new List<string> { "hiking", "camping", "wildlife viewing", "mountaineering", "scenic drives" }
                },
                new Park
                {
                    Id = "9",
                    Name = "Olympic National Park",
                    Description = "Diverse ecosystems from Pacific coastline to temperate rainforests to alpine areas, featuring hot springs and diverse wildlife.",
                    Location = "Washington, USA",
                    Latitude = 47.8021,
                    Longitude = -123.6044,
                    ParkType = "National Park",
                    Features = new List<string> { "rainforest", "coastline", "mountains", "hot springs", "diverse ecosystems" },
                    Activities = new List<string> { "hiking", "camping", "tide pooling", "hot springs", "rainforest walks" }
                },
                new Park
                {
                    Id = "10",
                    Name = "Glacier National Park",
                    Description = "Crown of the continent featuring pristine forests, alpine meadows, rugged mountains, and spectacular lakes. Home to diverse wildlife.",
                    Location = "Montana, USA",
                    Latitude = 48.7596,
                    Longitude = -113.7870,
                    ParkType = "National Park",
                    Features = new List<string> { "glaciers", "alpine meadows", "mountains", "pristine lakes", "wildlife" },
                    Activities = new List<string> { "hiking", "camping", "boat tours", "wildlife viewing", "scenic drives" }
                }
            };
        }
    }
}