using System.Text.Json;

namespace TripBuddy.API.Models
{
    public class SessionRequest
    {
        public string? UserId { get; set; }
        public string TripType { get; set; } = "backpacking";
        public string InitialTemplate { get; set; } = "backpacking-template-v1";
    }

    public class SessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public Template Template { get; set; } = new();
        public Dictionary<string, object> Context { get; set; } = new();
        public Dictionary<string, object> FormData { get; set; } = new();
        public string Guidance { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ContextUpdateRequest
    {
        public Dictionary<string, object> FieldUpdates { get; set; } = new();
        public bool TriggerLLM { get; set; } = false;
        public double CurrentProgress { get; set; } = 0.0;
    }

    public class ContextUpdateResponse
    {
        public string Guidance { get; set; } = string.Empty;
        public TemplateUpdates? TemplateUpdates { get; set; }
        public List<string> Suggestions { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class TemplateUpdates
    {
        public List<TemplateSection> NewSections { get; set; } = new();
        public List<string> ModifiedSections { get; set; } = new();
        public List<string> HiddenSections { get; set; } = new();
    }

    public class Template
    {
        public string TripType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<TemplateSection> Sections { get; set; } = new();
    }

    public class TemplateSection
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<FormField> Fields { get; set; } = new();
        public bool AiGenerated { get; set; } = false;
    }

    public class FormField
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Required { get; set; } = false;
        public string? Placeholder { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public List<FieldOption>? Options { get; set; }
    }

    public class FieldOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class TripPlanningSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public Template Template { get; set; } = new();
        public Dictionary<string, object> FormData { get; set; } = new();
        public LLMContext Context { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class LLMContext
    {
        public List<ConversationMessage> ConversationHistory { get; set; } = new();
        public ContextSummary Summary { get; set; } = new();
        public TemplateEvolution Evolution { get; set; } = new();
    }

    public class ConversationMessage
    {
        public string Role { get; set; } = string.Empty; // user, assistant, system
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Trigger { get; set; }
    }

    public class ContextSummary
    {
        public string TripType { get; set; } = string.Empty;
        public string? Destination { get; set; }
        public Dictionary<string, object> KeyDecisions { get; set; } = new();
        public List<string> UserPreferences { get; set; } = new();
        public List<string> WarningsGiven { get; set; } = new();
    }

    public class TemplateEvolution
    {
        public List<string> BaseSections { get; set; } = new();
        public List<string> AiAddedSections { get; set; } = new();
        public List<TemplateModification> ModificationHistory { get; set; } = new();
    }

    public class TemplateModification
    {
        public string Action { get; set; } = string.Empty; // add, modify, hide
        public string SectionId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class AIGuidanceRequest
    {
        public string Trigger { get; set; } = string.Empty;
        public AIGuidanceContext Context { get; set; } = new();
    }

    public class AIGuidanceContext
    {
        public string? ChangedField { get; set; }
        public object? NewValue { get; set; }
        public double FormProgress { get; set; }
    }

    public class AIGuidanceResponse
    {
        public string Guidance { get; set; } = string.Empty;
        public List<string> ActionItems { get; set; } = new();
        public DynamicContent? DynamicContent { get; set; }
    }

    public class DynamicContent
    {
        public List<TemplateSection> AddSections { get; set; } = new();
        public List<FormField> ModifyFields { get; set; } = new();
        public List<string> ShowWarnings { get; set; } = new();
    }

    public class ValidationRequest
    {
        public string Section { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public class ValidationResponse
    {
        public bool Valid { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class ValidationIssue
    {
        public string Field { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // error, warning, info
        public string Message { get; set; } = string.Empty;
        public string? Suggestion { get; set; }
    }

    public class FieldSuggestionRequest
    {
        public string FieldName { get; set; } = string.Empty;
        public string? CurrentValue { get; set; }
        public Dictionary<string, object> FormContext { get; set; } = new();
    }

    public class FieldSuggestionResponse
    {
        public string FieldName { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public string Explanation { get; set; } = string.Empty;
    }

    public class SuggestionsResponse
    {
        public List<FieldSuggestion> Suggestions { get; set; } = new();
        public List<string> AutoComplete { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class FieldSuggestion
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}