using System.Collections.Concurrent;
using System.Text.Json;
using TripBuddy.API.Models;

namespace TripBuddy.API.Services
{
    public class SessionService : ISessionService
    {
        private readonly ILogger<SessionService> _logger;
        private readonly ITextGenerationService _textGenerationService;

        // In-memory storage for demo purposes - use Redis/Database in production
        private static readonly ConcurrentDictionary<string, TripPlanningSession> _sessions = new();
        private static readonly Dictionary<string, Template> _templates = new();

        public SessionService(
            ILogger<SessionService> logger,
            ITextGenerationService textGenerationService)
        {
            _logger = logger;
            _textGenerationService = textGenerationService;
            InitializeTemplates();
        }

        public async Task<SessionResponse> CreateSessionAsync(SessionRequest request)
        {
            var sessionId = $"sess_{Guid.NewGuid():N}";
            var template = await GetTemplateAsync(request.InitialTemplate);

            var session = new TripPlanningSession
            {
                SessionId = sessionId,
                UserId = request.UserId,
                Template = template,
                FormData = new Dictionary<string, object>(),
                Context = new LLMContext
                {
                    Summary = new ContextSummary { TripType = request.TripType },
                    Evolution = new TemplateEvolution { BaseSections = template.Sections.Select(s => s.Id).ToList() }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 hour session timeout
            };

            _sessions[sessionId] = session;

            var guidance = await GenerateInitialGuidance(request.TripType);

            return new SessionResponse
            {
                SessionId = sessionId,
                Template = template,
                Context = new Dictionary<string, object> { { "tripType", request.TripType } },
                FormData = new Dictionary<string, object>(),
                Guidance = guidance,
                ExpiresAt = session.ExpiresAt,
                LastUpdated = session.UpdatedAt
            };
        }

        public async Task<SessionResponse?> GetSessionAsync(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return null;

            // Check if session has expired
            if (DateTime.UtcNow > session.ExpiresAt)
            {
                _sessions.TryRemove(sessionId, out _);
                return null;
            }

            var guidance = session.Context.ConversationHistory.LastOrDefault()?.Content ??
                          "Continue filling out your trip details for personalized recommendations.";

            return new SessionResponse
            {
                SessionId = session.SessionId,
                Template = session.Template,
                Context = ConvertToContextDictionary(session.Context),
                FormData = session.FormData,
                Guidance = guidance,
                ExpiresAt = session.ExpiresAt,
                LastUpdated = session.UpdatedAt
            };
        }

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            return _sessions.TryRemove(sessionId, out _);
        }

        public async Task<ContextUpdateResponse> UpdateContextAsync(string sessionId, ContextUpdateRequest request)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                throw new ArgumentException("Session not found", nameof(sessionId));

            // Update form data
            foreach (var update in request.FieldUpdates)
            {
                session.FormData[update.Key] = update.Value;
                session.Context.Summary.KeyDecisions[update.Key] = update.Value;
            }

            session.UpdatedAt = DateTime.UtcNow;

            var response = new ContextUpdateResponse();

            if (request.TriggerLLM)
            {
                response = await ProcessLLMUpdate(session, request);

                // Add conversation to history
                session.Context.ConversationHistory.Add(new ConversationMessage
                {
                    Role = "assistant",
                    Content = response.Guidance,
                    Timestamp = DateTime.UtcNow,
                    Trigger = string.Join(", ", request.FieldUpdates.Keys)
                });
            }

            _sessions[sessionId] = session;
            return response;
        }

        public async Task<FieldSuggestionResponse> GetFieldSuggestionAsync(string sessionId, FieldSuggestionRequest request)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                throw new ArgumentException("Session not found", nameof(sessionId));

            var prompt = BuildFieldSuggestionPrompt(session, request);

            try
            {
                var aiResponse = await _textGenerationService.GenerateResponseAsync(prompt);

                return new FieldSuggestionResponse
                {
                    Suggestions = ParseSuggestions(aiResponse),
                    Explanation = ExtractExplanation(aiResponse),
                    FieldName = request.FieldName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get field suggestion for session {SessionId}, field {FieldName}", sessionId, request.FieldName);
                return new FieldSuggestionResponse
                {
                    Suggestions = new List<string>(),
                    Explanation = "Unable to generate suggestions at this time.",
                    FieldName = request.FieldName
                };
            }
        }

        public async Task<AIGuidanceResponse> GetAIGuidanceAsync(string sessionId, AIGuidanceRequest request)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                throw new ArgumentException("Session not found", nameof(sessionId));

            var prompt = BuildGuidancePrompt(session, request);

            try
            {
                var aiResponse = await _textGenerationService.GenerateResponseAsync(prompt);

                return new AIGuidanceResponse
                {
                    Guidance = aiResponse,
                    ActionItems = ExtractActionItems(aiResponse),
                    DynamicContent = await GenerateDynamicContent(session, request)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get AI guidance for session {SessionId}", sessionId);
                return new AIGuidanceResponse
                {
                    Guidance = "Continue with your trip planning. I'm here to help!",
                    ActionItems = new List<string>()
                };
            }
        }

        public async Task<ValidationResponse> ValidateDataAsync(string sessionId, ValidationRequest request)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                throw new ArgumentException("Session not found", nameof(sessionId));

            var issues = new List<ValidationIssue>();

            // Example validation logic - extend based on your needs
            if (request.Section == "gear" && request.Data.ContainsKey("shelter") && request.Data.ContainsKey("season"))
            {
                var shelter = request.Data["shelter"]?.ToString();
                var season = request.Data["season"]?.ToString();

                if (shelter == "tent_3season" && season == "winter")
                {
                    issues.Add(new ValidationIssue
                    {
                        Field = "shelter",
                        Severity = "error",
                        Message = "3-season tent insufficient for winter conditions",
                        Suggestion = "Consider 4-season tent or winter shelter"
                    });
                }
            }

            return new ValidationResponse
            {
                Valid = !issues.Any(i => i.Severity == "error"),
                Issues = issues,
                Recommendations = await GenerateRecommendations(session, request)
            };
        }

        public async Task<SuggestionsResponse> GetSuggestionsAsync(string sessionId, string fieldName, string? currentValue = null)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                throw new ArgumentException("Session not found", nameof(sessionId));

            return fieldName switch
            {
                "destination" => await GetDestinationSuggestions(session, currentValue),
                "shelter" => await GetShelterSuggestions(session, currentValue),
                _ => new SuggestionsResponse()
            };
        }

        public async Task<Template> GetTemplateAsync(string templateId)
        {
            if (_templates.TryGetValue(templateId, out var template))
                return template;

            throw new ArgumentException($"Template {templateId} not found", nameof(templateId));
        }

        private void InitializeTemplates()
        {
            var backpackingTemplate = new Template
            {
                TripType = "backpacking",
                Title = "Backpacking Trip Planner",
                Sections = new List<TemplateSection>
                {
                    new TemplateSection
                    {
                        Id = "basics",
                        Title = "Trip Basics",
                        Description = "Essential information about your trip",
                        Fields = new List<FormField>
                        {
                            new FormField { Name = "destination", Type = "text", Label = "Park/Destination", Required = true, Placeholder = "e.g., Yosemite National Park" },
                            new FormField { Name = "duration", Type = "number", Label = "Trip Duration (days)", Required = true, Min = 1, Max = 14 },
                            new FormField { Name = "group_size", Type = "number", Label = "Group Size", Required = true, Min = 1, Max = 12 },
                            new FormField
                            {
                                Name = "season", Type = "select", Label = "Season", Required = true,
                                Options = new List<FieldOption>
                                {
                                    new FieldOption { Value = "spring", Label = "Spring (Mar-May)" },
                                    new FieldOption { Value = "summer", Label = "Summer (Jun-Aug)" },
                                    new FieldOption { Value = "fall", Label = "Fall (Sep-Nov)" },
                                    new FieldOption { Value = "winter", Label = "Winter (Dec-Feb)" }
                                }
                            }
                        }
                    },
                    new TemplateSection
                    {
                        Id = "gear",
                        Title = "Essential Gear",
                        Description = "Core equipment needed for your trip",
                        Fields = new List<FormField>
                        {
                            new FormField
                            {
                                Name = "shelter", Type = "select", Label = "Shelter Type", Required = true,
                                Options = new List<FieldOption>
                                {
                                    new FieldOption { Value = "tent_3season", Label = "3-Season Tent" },
                                    new FieldOption { Value = "tent_4season", Label = "4-Season Tent" },
                                    new FieldOption { Value = "tarp", Label = "Tarp/Bivy" }
                                }
                            },
                            new FormField
                            {
                                Name = "sleeping_bag", Type = "select", Label = "Sleeping Bag Rating", Required = true,
                                Options = new List<FieldOption>
                                {
                                    new FieldOption { Value = "summer", Label = "Summer (40°F+)" },
                                    new FieldOption { Value = "three_season", Label = "3-Season (20°F)" },
                                    new FieldOption { Value = "winter", Label = "Winter (0°F or below)" }
                                }
                            },
                            new FormField
                            {
                                Name = "cooking", Type = "checkbox", Label = "Cooking Equipment",
                                Options = new List<FieldOption>
                                {
                                    new FieldOption { Value = "stove", Label = "Backpacking stove" },
                                    new FieldOption { Value = "cookset", Label = "Lightweight cookset" },
                                    new FieldOption { Value = "water_filter", Label = "Water filter/purification" }
                                }
                            }
                        }
                    },
                    new TemplateSection
                    {
                        Id = "safety",
                        Title = "Safety & Permits",
                        Description = "Important safety considerations and permits",
                        Fields = new List<FormField>
                        {
                            new FormField { Name = "permits_needed", Type = "checkbox", Label = "Permits Required" },
                            new FormField { Name = "emergency_contact", Type = "text", Label = "Emergency Contact", Placeholder = "Name and phone number" },
                            new FormField { Name = "first_aid", Type = "checkbox", Label = "First Aid Kit" },
                            new FormField { Name = "navigation", Type = "checkbox", Label = "Navigation Tools (map, compass, GPS)" }
                        }
                    }
                }
            };

            _templates["backpacking-template-v1"] = backpackingTemplate;
        }

        private async Task<string> GenerateInitialGuidance(string tripType)
        {
            var prompt = $"Generate a welcoming message for someone starting to plan a {tripType} trip. Keep it brief and encouraging.";

            try
            {
                return await _textGenerationService.GenerateResponseAsync(prompt);
            }
            catch
            {
                return $"Welcome! Let's plan your {tripType} trip. Start by telling me your destination and I'll provide personalized recommendations.";
            }
        }

        private async Task<ContextUpdateResponse> ProcessLLMUpdate(TripPlanningSession session, ContextUpdateRequest request)
        {
            var response = new ContextUpdateResponse();

            // Determine if we need to add destination-specific sections
            if (request.FieldUpdates.ContainsKey("destination"))
            {
                var destination = request.FieldUpdates["destination"]?.ToString()?.ToLower();
                if (!string.IsNullOrEmpty(destination))
                {
                    if (destination.Contains("yosemite"))
                    {
                        response = await AddYosemiteSpecificSections(session);
                    }
                    else if (destination.Contains("yellowstone"))
                    {
                        response = await AddYellowstoneSpecificSections(session);
                    }
                }
            }

            // Handle gear-related updates
            if (request.FieldUpdates.ContainsKey("shelter") ||
                request.FieldUpdates.ContainsKey("sleeping_bag") ||
                request.FieldUpdates.ContainsKey("cooking"))
            {
                response = await ProcessGearUpdates(session, request, response);
            }

            // Generate contextual guidance
            var guidancePrompt = BuildContextualPrompt(session, request);
            try
            {
                response.Guidance = await _textGenerationService.GenerateResponseAsync(guidancePrompt);
            }
            catch
            {
                response.Guidance = "Great choice! I've updated your planning recommendations based on your selection.";
            }

            return response;
        }

        private async Task<ContextUpdateResponse> AddYosemiteSpecificSections(TripPlanningSession session)
        {
            var yosemiteSection = new TemplateSection
            {
                Id = "yosemite_specific",
                Title = "Yosemite Requirements",
                Description = "Specific requirements for Yosemite National Park",
                AiGenerated = true,
                Fields = new List<FormField>
                {
                    new FormField { Name = "wilderness_permit", Type = "checkbox", Label = "Wilderness permit obtained from recreation.gov", Required = true },
                    new FormField { Name = "bear_canister", Type = "checkbox", Label = "Bear canister (required in wilderness)", Required = true },
                    new FormField { Name = "parking_reservation", Type = "checkbox", Label = "Day-use parking reservation (if applicable)" }
                }
            };

            session.Template.Sections.Add(yosemiteSection);
            session.Context.Evolution.AiAddedSections.Add("yosemite_specific");
            session.Context.Evolution.ModificationHistory.Add(new TemplateModification
            {
                Action = "add",
                SectionId = "yosemite_specific",
                Reason = "Yosemite destination selected",
                Timestamp = DateTime.UtcNow
            });

            return new ContextUpdateResponse
            {
                Guidance = "Great choice! Yosemite offers incredible backpacking opportunities. I've added specific requirements for Yosemite wilderness permits and bear safety.",
                TemplateUpdates = new TemplateUpdates
                {
                    NewSections = new List<TemplateSection> { yosemiteSection }
                },
                Suggestions = new List<string>
                {
                    "Book wilderness permits early - they fill up quickly",
                    "Consider bear canisters from REI or local outfitters"
                }
            };
        }

        private async Task<ContextUpdateResponse> AddYellowstoneSpecificSections(TripPlanningSession session)
        {
            var yellowstoneSection = new TemplateSection
            {
                Id = "yellowstone_specific",
                Title = "Yellowstone Requirements",
                Description = "Specific requirements for Yellowstone National Park",
                AiGenerated = true,
                Fields = new List<FormField>
                {
                    new FormField { Name = "backcountry_permit", Type = "checkbox", Label = "Backcountry permit obtained", Required = true },
                    new FormField { Name = "bear_spray", Type = "checkbox", Label = "Bear spray (strongly recommended)", Required = true },
                    new FormField { Name = "thermal_awareness", Type = "checkbox", Label = "Reviewed thermal feature safety guidelines" }
                }
            };

            session.Template.Sections.Add(yellowstoneSection);
            session.Context.Evolution.AiAddedSections.Add("yellowstone_specific");

            return new ContextUpdateResponse
            {
                Guidance = "Yellowstone is an amazing destination! I've added specific requirements including bear safety measures and thermal feature awareness.",
                TemplateUpdates = new TemplateUpdates
                {
                    NewSections = new List<TemplateSection> { yellowstoneSection }
                }
            };
        }

        private async Task<ContextUpdateResponse> ProcessGearUpdates(TripPlanningSession session, ContextUpdateRequest request, ContextUpdateResponse response)
        {
            var season = session.FormData.ContainsKey("season") ? session.FormData["season"]?.ToString() : null;
            var destination = session.FormData.ContainsKey("destination") ? session.FormData["destination"]?.ToString() : null;

            // Check for gear compatibility issues and provide warnings
            if (request.FieldUpdates.ContainsKey("shelter"))
            {
                var shelter = request.FieldUpdates["shelter"]?.ToString();
                if (shelter == "tent_3season" && season == "winter")
                {
                    response.Warnings.Add("⚠️ 3-season tent may not be suitable for winter conditions. Consider a 4-season tent for better weather protection.");
                }

                if (shelter == "tarp" && !string.IsNullOrEmpty(destination) && destination.ToLower().Contains("alaska"))
                {
                    response.Warnings.Add("⚠️ Tarp camping in Alaska requires advanced skills. Consider bringing backup shelter.");
                }
            }

            if (request.FieldUpdates.ContainsKey("sleeping_bag"))
            {
                var sleepingBag = request.FieldUpdates["sleeping_bag"]?.ToString();
                if (sleepingBag == "summer" && season == "winter")
                {
                    response.Warnings.Add("❄️ Summer sleeping bag (40°F+) is insufficient for winter camping. You'll need a winter-rated bag (0°F or lower).");
                }

                if (sleepingBag == "winter" && season == "summer")
                {
                    response.Suggestions.Add("💡 Winter sleeping bag might be too warm for summer. Consider a lighter option for comfort.");
                }
            }

            if (request.FieldUpdates.ContainsKey("cooking"))
            {
                var cooking = request.FieldUpdates["cooking"] as string[] ?? new string[0];
                if (cooking.Contains("stove") && season == "winter")
                {
                    response.Suggestions.Add("🔥 For winter camping, liquid fuel stoves perform better than canister stoves in cold weather.");
                }

                if (!cooking.Contains("water_filter") && !string.IsNullOrEmpty(destination))
                {
                    response.Suggestions.Add("💧 Don't forget water treatment! Most wilderness areas require water purification.");
                }
            }

            // Add gear-specific sections based on conditions
            if (season == "winter" && !session.Template.Sections.Any(s => s.Id == "winter_gear"))
            {
                var winterGearSection = new TemplateSection
                {
                    Id = "winter_gear",
                    Title = "Winter-Specific Gear",
                    Description = "Additional gear needed for winter conditions",
                    AiGenerated = true,
                    Fields = new List<FormField>
                    {
                        new FormField { Name = "insulation_layers", Type = "checkbox", Label = "Insulation layers (down jacket, fleece)", Required = true },
                        new FormField { Name = "winter_boots", Type = "checkbox", Label = "Insulated boots with good traction", Required = true },
                        new FormField { Name = "microspikes", Type = "checkbox", Label = "Microspikes or crampons for icy conditions" },
                        new FormField { Name = "hand_foot_warmers", Type = "checkbox", Label = "Hand and foot warmers" },
                        new FormField { Name = "emergency_bivy", Type = "checkbox", Label = "Emergency bivy or space blanket" }
                    }
                };

                session.Template.Sections.Add(winterGearSection);
                session.Context.Evolution.AiAddedSections.Add("winter_gear");
                response.TemplateUpdates = response.TemplateUpdates ?? new TemplateUpdates();
                response.TemplateUpdates.NewSections.Add(winterGearSection);

                response.Suggestions.Add("❄️ Added winter-specific gear section due to season selection.");
            }

            return response;
        }

        private string BuildGuidancePrompt(TripPlanningSession session, AIGuidanceRequest request)
        {
            var context = $"Trip Type: {session.Context.Summary.TripType}\n";
            if (session.Context.Summary.Destination != null)
                context += $"Destination: {session.Context.Summary.Destination}\n";

            context += $"Changed Field: {request.Context.ChangedField}\n";
            context += $"New Value: {request.Context.NewValue}\n";
            context += $"Form Progress: {request.Context.FormProgress:P}\n";

            return $"You are a helpful trip planning assistant. Based on this context:\n{context}\n\nProvide brief, actionable guidance (2-3 sentences max) for the user's trip planning.";
        }

        private string BuildContextualPrompt(TripPlanningSession session, ContextUpdateRequest request)
        {
            var updates = string.Join(", ", request.FieldUpdates.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

            // Add current context for better AI guidance
            var context = $"Trip context: ";
            var contextParts = new List<string>();

            if (session.FormData.ContainsKey("destination"))
                contextParts.Add($"destination={session.FormData["destination"]}");
            if (session.FormData.ContainsKey("season"))
                contextParts.Add($"season={session.FormData["season"]}");
            if (session.FormData.ContainsKey("experience_level"))
                contextParts.Add($"experience={session.FormData["experience_level"]}");

            if (contextParts.Any())
                context += string.Join(", ", contextParts) + ". ";

            // Check if this is a gear-related update
            var gearFields = new[] { "shelter", "sleeping_bag", "cooking" };
            var isGearUpdate = request.FieldUpdates.Keys.Any(key => gearFields.Contains(key));

            if (isGearUpdate)
            {
                return $"{context}User updated gear selection: {updates}. Provide specific guidance about gear compatibility, safety considerations, or recommendations for their trip conditions. Keep it brief (1-2 sentences).";
            }

            return $"{context}User updated their trip planning with: {updates}. Provide encouraging, specific guidance in 1-2 sentences.";
        }

        private string BuildFieldSuggestionPrompt(TripPlanningSession session, FieldSuggestionRequest request)
        {
            var contextInfo = BuildContextInfo(session);

            return $@"
You are an expert trip planning assistant. The user is filling out a form for a {session.Context.Summary.TripType} trip and needs suggestions for the '{request.FieldName}' field.

Current form context:
{contextInfo}

Field being filled: {request.FieldName}
Current value: {request.CurrentValue ?? "empty"}

Based on the trip details already provided, suggest 3-5 specific, practical recommendations for this field. 
Explain briefly why each suggestion fits their trip profile.

Format your response as:
SUGGESTIONS:
- [suggestion 1]
- [suggestion 2]
- [suggestion 3]

EXPLANATION:
[Brief explanation of why these suggestions fit their trip]";
        }

        private string BuildContextInfo(TripPlanningSession session)
        {
            var contextLines = new List<string>();

            contextLines.Add($"Trip Type: {session.Context.Summary.TripType}");

            foreach (var decision in session.Context.Summary.KeyDecisions)
            {
                contextLines.Add($"{decision.Key}: {decision.Value}");
            }

            if (session.FormData.Any())
            {
                contextLines.Add("Current form data:");
                foreach (var item in session.FormData)
                {
                    contextLines.Add($"  {item.Key}: {item.Value}");
                }
            }

            return string.Join("\n", contextLines);
        }

        private List<string> ParseSuggestions(string aiResponse)
        {
            var suggestions = new List<string>();
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            bool inSuggestionsSection = false;

            foreach (var line in lines)
            {
                if (line.Trim().ToUpper().StartsWith("SUGGESTIONS:"))
                {
                    inSuggestionsSection = true;
                    continue;
                }

                if (line.Trim().ToUpper().StartsWith("EXPLANATION:"))
                {
                    inSuggestionsSection = false;
                    continue;
                }

                if (inSuggestionsSection && (line.Trim().StartsWith("-") || line.Trim().StartsWith("•")))
                {
                    var suggestion = line.Trim().TrimStart('-', '•').Trim();
                    if (!string.IsNullOrWhiteSpace(suggestion))
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }

            return suggestions.Take(5).ToList();
        }

        private string ExtractExplanation(string aiResponse)
        {
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            bool inExplanationSection = false;
            var explanationLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.Trim().ToUpper().StartsWith("EXPLANATION:"))
                {
                    inExplanationSection = true;
                    continue;
                }

                if (inExplanationSection)
                {
                    explanationLines.Add(line.Trim());
                }
            }

            return string.Join(" ", explanationLines).Trim();
        }

        private List<string> ExtractActionItems(string aiResponse)
        {
            // Simple extraction - could be enhanced with better parsing
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            return lines.Where(line => line.Contains("•") || line.Contains("-") || line.Contains("*"))
                       .Take(3)
                       .ToList();
        }

        private async Task<DynamicContent?> GenerateDynamicContent(TripPlanningSession session, AIGuidanceRequest request)
        {
            // Add logic to generate dynamic content based on context
            return null;
        }

        private async Task<List<string>> GenerateRecommendations(TripPlanningSession session, ValidationRequest request)
        {
            return new List<string> { "Consider checking weather conditions", "Review gear list thoroughly" };
        }

        private async Task<SuggestionsResponse> GetDestinationSuggestions(TripPlanningSession session, string? currentValue)
        {
            return new SuggestionsResponse
            {
                Suggestions = new List<FieldSuggestion>
                {
                    new FieldSuggestion { Value = "Yosemite National Park", Label = "Yosemite National Park", Reason = "Popular backpacking destination", Confidence = 0.9 },
                    new FieldSuggestion { Value = "Yellowstone National Park", Label = "Yellowstone National Park", Reason = "Diverse wilderness opportunities", Confidence = 0.85 }
                }
            };
        }

        private async Task<SuggestionsResponse> GetShelterSuggestions(TripPlanningSession session, string? currentValue)
        {
            var season = session.FormData.ContainsKey("season") ? session.FormData["season"]?.ToString() : null;

            if (season == "winter")
            {
                return new SuggestionsResponse
                {
                    Suggestions = new List<FieldSuggestion>
                    {
                        new FieldSuggestion { Value = "tent_4season", Label = "4-Season Mountaineering Tent", Reason = "Handles snow load and wind", Confidence = 0.95 }
                    },
                    Warnings = new List<string> { "Winter conditions require specialized gear" }
                };
            }

            return new SuggestionsResponse();
        }

        private Dictionary<string, object> ConvertToContextDictionary(LLMContext context)
        {
            return new Dictionary<string, object>
            {
                { "tripType", context.Summary.TripType },
                { "destination", context.Summary.Destination ?? "" },
                { "keyDecisions", context.Summary.KeyDecisions },
                { "conversationCount", context.ConversationHistory.Count }
            };
        }
    }
}