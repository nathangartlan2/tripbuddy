using TripBuddy.API.Models;

namespace TripBuddy.API.Services
{
    public interface ISessionService
    {
        Task<SessionResponse> CreateSessionAsync(SessionRequest request);
        Task<SessionResponse?> GetSessionAsync(string sessionId);
        Task<bool> DeleteSessionAsync(string sessionId);
        Task<ContextUpdateResponse> UpdateContextAsync(string sessionId, ContextUpdateRequest request);
        Task<AIGuidanceResponse> GetAIGuidanceAsync(string sessionId, AIGuidanceRequest request);
        Task<FieldSuggestionResponse> GetFieldSuggestionAsync(string sessionId, FieldSuggestionRequest request);
        Task<ValidationResponse> ValidateDataAsync(string sessionId, ValidationRequest request);
        Task<SuggestionsResponse> GetSuggestionsAsync(string sessionId, string fieldName, string? currentValue = null);
        Task<Template> GetTemplateAsync(string templateId);
    }
}