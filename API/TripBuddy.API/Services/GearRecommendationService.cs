using TripBuddy.API.Data;
using TripBuddy.API.Models;
using TripBuddy.API.Services;

namespace TripBuddy.API.Services
{
    public interface IGearRecommendationService
    {
        Task<GenerateGearListResponse> GenerateCustomGearListAsync(GenerateGearListRequest request);
    }

    public class GearRecommendationService : IGearRecommendationService
    {
        private readonly ITextGenerationService _textGenerationService;
        private readonly IGearTemplateService _gearTemplateService;
        private readonly ILogger<GearRecommendationService> _logger;

        public GearRecommendationService(
            ITextGenerationService textGenerationService,
            IGearTemplateService gearTemplateService,
            ILogger<GearRecommendationService> logger)
        {
            _textGenerationService = textGenerationService;
            _gearTemplateService = gearTemplateService;
            _logger = logger;
        }

        public async Task<GenerateGearListResponse> GenerateCustomGearListAsync(GenerateGearListRequest request)
        {
            try
            {
                // Get base gear template
                var baseTemplate = _gearTemplateService.GetTemplateByTripType(request.TripContext.TripType);

                // Create AI prompt to modify gear list
                var prompt = BuildGearModificationPrompt(baseTemplate, request.TripContext);

                // Get AI recommendations
                var aiResponse = await _textGenerationService.GenerateResponseAsync(prompt);

                // Parse AI response and create modified gear list
                var modifiedGearList = await ProcessAIGearRecommendations(baseTemplate, request.TripContext, aiResponse);

                return new GenerateGearListResponse
                {
                    GearList = modifiedGearList,
                    Summary = ExtractSummary(aiResponse),
                    Warnings = ExtractWarnings(aiResponse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate custom gear list");

                // Fallback: return base template with no modifications
                var fallbackGearList = CreateBasicGearList(BaseGearData.GetTemplateByTripType(request.TripContext.TripType), request.TripContext);

                return new GenerateGearListResponse
                {
                    GearList = fallbackGearList,
                    Summary = "Using standard gear list (AI recommendations unavailable)",
                    Warnings = new List<string> { "Unable to provide custom recommendations at this time" }
                };
            }
        }

        private string BuildGearModificationPrompt(BaseGearTemplate baseTemplate, TripContext context)
        {
            var baseItemsList = string.Join("\n",
                baseTemplate.Categories.SelectMany(c =>
                    c.BaseItems.Select(item => $"- {item} ({c.Name})")
                )
            );

            return $@"
You are an expert outdoor gear advisor. Based on the trip details, modify this base gear list by adding, removing, or upgrading items.

TRIP CONTEXT:
- Destination: {context.Destination}
- Season: {context.Season}
- Duration: {context.Duration}
- Experience Level: {context.ExperienceLevel}
- Group Size: {context.GroupSize}
- Trip Type: {context.TripType}

BASE GEAR LIST:
{baseItemsList}

INSTRUCTIONS:
1. For each modification, provide a brief reason (1-2 sentences max)
2. Focus on safety, weather conditions, and trip-specific needs
3. Consider the user's experience level

FORMAT YOUR RESPONSE AS:
KEEP: [list items to keep unchanged]
ADD: [item name] - [brief reason]
REMOVE: [item name] - [brief reason]
UPGRADE: [original item] → [better item] - [brief reason]

SUMMARY: [2-3 sentence overview of key changes]
WARNINGS: [any important safety or logistics alerts]
";
        }

        private async Task<GearList> ProcessAIGearRecommendations(BaseGearTemplate baseTemplate, TripContext context, string aiResponse)
        {
            var gearList = new GearList
            {
                Context = context,
                GeneratedAt = DateTime.UtcNow,
                Items = new List<GearItem>()
            };

            // Start with base items
            foreach (var category in baseTemplate.Categories)
            {
                foreach (var item in category.BaseItems)
                {
                    gearList.Items.Add(new GearItem
                    {
                        Name = item,
                        Category = category.Name,
                        IsRequired = category.IsEssential,
                        Modification = null // Base item, no modification
                    });
                }
            }

            // Parse AI response and apply modifications
            var modifications = ParseAIModifications(aiResponse);

            foreach (var mod in modifications)
            {
                ApplyModification(gearList, mod);
            }

            return gearList;
        }

        private List<ParsedModification> ParseAIModifications(string aiResponse)
        {
            var modifications = new List<ParsedModification>();
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            string currentSection = "";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("ADD:"))
                {
                    currentSection = "ADD";
                    var content = trimmed.Substring(4).Trim();
                    var modification = ParseModificationLine(content, "added");
                    if (modification != null) modifications.Add(modification);
                }
                else if (trimmed.StartsWith("REMOVE:"))
                {
                    currentSection = "REMOVE";
                    var content = trimmed.Substring(7).Trim();
                    var modification = ParseModificationLine(content, "removed");
                    if (modification != null) modifications.Add(modification);
                }
                else if (trimmed.StartsWith("UPGRADE:"))
                {
                    currentSection = "UPGRADE";
                    var content = trimmed.Substring(8).Trim();
                    var modification = ParseUpgradeLine(content);
                    if (modification != null) modifications.Add(modification);
                }
                else if (currentSection != "" && trimmed.StartsWith("-"))
                {
                    // Continuation of current section
                    var content = trimmed.Substring(1).Trim();
                    var action = currentSection.ToLower();

                    ParsedModification? modification = action switch
                    {
                        "add" => ParseModificationLine(content, "added"),
                        "remove" => ParseModificationLine(content, "removed"),
                        "upgrade" => ParseUpgradeLine(content),
                        _ => null
                    };

                    if (modification != null) modifications.Add(modification);
                }
            }

            return modifications;
        }

        private ParsedModification? ParseModificationLine(string content, string action)
        {
            var parts = content.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return new ParsedModification
                {
                    Action = action,
                    ItemName = parts[0].Trim(),
                    Reason = parts[1].Trim()
                };
            }
            return null;
        }

        private ParsedModification? ParseUpgradeLine(string content)
        {
            if (content.Contains("→"))
            {
                var parts = content.Split('→', 2);
                if (parts.Length == 2)
                {
                    var secondPart = parts[1].Split('-', 2);
                    if (secondPart.Length >= 2)
                    {
                        return new ParsedModification
                        {
                            Action = "upgraded",
                            ItemName = secondPart[0].Trim(),
                            OriginalItem = parts[0].Trim(),
                            Reason = secondPart[1].Trim()
                        };
                    }
                }
            }
            return null;
        }

        private void ApplyModification(GearList gearList, ParsedModification mod)
        {
            switch (mod.Action)
            {
                case "added":
                    gearList.Items.Add(new GearItem
                    {
                        Name = mod.ItemName,
                        Category = "Additional",
                        IsRequired = true,
                        Note = mod.Reason,
                        Modification = new GearModification
                        {
                            Action = "added",
                            Reason = mod.Reason
                        }
                    });
                    break;

                case "removed":
                    var itemToRemove = gearList.Items.FirstOrDefault(i =>
                        i.Name.Contains(mod.ItemName, StringComparison.OrdinalIgnoreCase) ||
                        mod.ItemName.Contains(i.Name, StringComparison.OrdinalIgnoreCase));

                    if (itemToRemove != null)
                    {
                        itemToRemove.Modification = new GearModification
                        {
                            Action = "removed",
                            Reason = mod.Reason
                        };
                        itemToRemove.Note = $"Removed: {mod.Reason}";
                        itemToRemove.IsRequired = false;
                    }
                    break;

                case "upgraded":
                    var itemToUpgrade = gearList.Items.FirstOrDefault(i =>
                        i.Name.Contains(mod.OriginalItem ?? "", StringComparison.OrdinalIgnoreCase));

                    if (itemToUpgrade != null)
                    {
                        var originalName = itemToUpgrade.Name;
                        itemToUpgrade.Name = mod.ItemName;
                        itemToUpgrade.Modification = new GearModification
                        {
                            Action = "upgraded",
                            Reason = mod.Reason,
                            ReplacedItem = originalName
                        };
                        itemToUpgrade.Note = $"Upgraded from {originalName}: {mod.Reason}";
                    }
                    break;
            }
        }

        private GearList CreateBasicGearList(BaseGearTemplate template, TripContext context)
        {
            var gearList = new GearList
            {
                Context = context,
                GeneratedAt = DateTime.UtcNow,
                Items = new List<GearItem>()
            };

            foreach (var category in template.Categories)
            {
                foreach (var item in category.BaseItems)
                {
                    gearList.Items.Add(new GearItem
                    {
                        Name = item,
                        Category = category.Name,
                        IsRequired = category.IsEssential
                    });
                }
            }

            return gearList;
        }

        private string ExtractSummary(string aiResponse)
        {
            var lines = aiResponse.Split('\n');
            var summaryLine = lines.FirstOrDefault(l => l.Trim().StartsWith("SUMMARY:"));
            return summaryLine?.Substring(8).Trim() ?? "Custom gear recommendations generated based on your trip details.";
        }

        private List<string> ExtractWarnings(string aiResponse)
        {
            var warnings = new List<string>();
            var lines = aiResponse.Split('\n');
            var warningsLine = lines.FirstOrDefault(l => l.Trim().StartsWith("WARNINGS:"));

            if (warningsLine != null)
            {
                var warningText = warningsLine.Substring(9).Trim();
                if (!string.IsNullOrEmpty(warningText))
                {
                    warnings.Add(warningText);
                }
            }

            return warnings;
        }

        private class ParsedModification
        {
            public string Action { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public string? OriginalItem { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}