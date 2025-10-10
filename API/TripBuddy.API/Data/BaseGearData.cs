using TripBuddy.API.Models;

namespace TripBuddy.API.Data
{
    public static class BaseGearData
    {
        // Standard backpacking gear list that AI will modify
        public static BaseGearTemplate GetBackpackingTemplate() => new()
        {
            TripType = "backpacking",
            Categories = new List<GearCategory>
            {
                new()
                {
                    Name = "Shelter",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Tent or tarp",
                        "Sleeping bag",
                        "Sleeping pad",
                        "Pillow or stuff sack"
                    }
                },
                new()
                {
                    Name = "Cooking",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Stove and fuel",
                        "Cookpot",
                        "Spork or utensils",
                        "Water bottles",
                        "Water filter or purification"
                    }
                },
                new()
                {
                    Name = "Clothing",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Hiking boots",
                        "Moisture-wicking base layer",
                        "Insulating layer",
                        "Rain jacket",
                        "Extra socks and underwear",
                        "Hat and gloves"
                    }
                },
                new()
                {
                    Name = "Safety & Navigation",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Map and compass",
                        "First aid kit",
                        "Headlamp and extra batteries",
                        "Emergency whistle",
                        "Sunscreen and sunglasses"
                    }
                },
                new()
                {
                    Name = "Pack & Organization",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Backpack",
                        "Stuff sacks or packing cubes",
                        "Trash bags",
                        "Rope or paracord"
                    }
                },
                new()
                {
                    Name = "Personal Care",
                    IsEssential = false,
                    BaseItems = new()
                    {
                        "Toothbrush and toothpaste",
                        "Toilet paper",
                        "Trowel",
                        "Personal medications"
                    }
                }
            }
        };

        public static BaseGearTemplate GetDayHikingTemplate() => new()
        {
            TripType = "day_hiking",
            Categories = new List<GearCategory>
            {
                new()
                {
                    Name = "Essentials",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Day pack",
                        "Water bottles",
                        "Snacks and lunch",
                        "Map and compass",
                        "First aid kit",
                        "Headlamp"
                    }
                },
                new()
                {
                    Name = "Clothing",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Hiking boots",
                        "Moisture-wicking layers",
                        "Rain jacket",
                        "Hat and sunglasses"
                    }
                },
                new()
                {
                    Name = "Optional",
                    IsEssential = false,
                    BaseItems = new()
                    {
                        "Trekking poles",
                        "Camera",
                        "Binoculars",
                        "Field guides"
                    }
                }
            }
        };

        public static BaseGearTemplate GetCarCampingTemplate() => new()
        {
            TripType = "car_camping",
            Categories = new List<GearCategory>
            {
                new()
                {
                    Name = "Shelter",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Tent",
                        "Sleeping bag",
                        "Sleeping pad or air mattress",
                        "Pillow",
                        "Camping chairs"
                    }
                },
                new()
                {
                    Name = "Cooking",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Camping stove or grill",
                        "Fuel",
                        "Cookware and utensils",
                        "Cooler and ice",
                        "Water containers",
                        "Plates and cups"
                    }
                },
                new()
                {
                    Name = "Clothing",
                    IsEssential = true,
                    BaseItems = new()
                    {
                        "Weather-appropriate clothing",
                        "Rain gear",
                        "Extra clothes",
                        "Comfortable camp shoes"
                    }
                },
                new()
                {
                    Name = "Comfort & Entertainment",
                    IsEssential = false,
                    BaseItems = new()
                    {
                        "Lantern",
                        "Portable table",
                        "Games or books",
                        "Portable speaker"
                    }
                }
            }
        };

        public static BaseGearTemplate GetTemplateByTripType(string tripType) => tripType.ToLower() switch
        {
            "backpacking" => GetBackpackingTemplate(),
            "day_hiking" => GetDayHikingTemplate(),
            "car_camping" => GetCarCampingTemplate(),
            _ => GetBackpackingTemplate() // Default fallback
        };
    }
}