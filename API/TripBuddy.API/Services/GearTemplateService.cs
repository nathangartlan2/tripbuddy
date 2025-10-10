using TripBuddy.API.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TripBuddy.API.Data
{
    public interface IGearTemplateService
    {
        BaseGearTemplate GetTemplateByTripType(string tripType);
        IEnumerable<BaseGearTemplate> GetAllTemplates();
    }

    public class GearTemplateService : IGearTemplateService
    {
        private readonly Dictionary<string, BaseGearTemplate> _templates;
        private readonly ILogger<GearTemplateService> _logger;

        public GearTemplateService(ILogger<GearTemplateService> logger)
        {
            _logger = logger;
            _templates = new Dictionary<string, BaseGearTemplate>();
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            try
            {
                var templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "GearTemplates");

                if (!Directory.Exists(templatesPath))
                {
                    _logger.LogError("GearTemplates directory not found at: {Path}", templatesPath);
                    return;
                }

                var yamlFiles = Directory.GetFiles(templatesPath, "*.yaml");
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                foreach (var file in yamlFiles)
                {
                    try
                    {
                        var yamlContent = File.ReadAllText(file);
                        var template = deserializer.Deserialize<BaseGearTemplate>(yamlContent);

                        if (template != null && !string.IsNullOrEmpty(template.TripType))
                        {
                            _templates[template.TripType.ToLower()] = template;
                            _logger.LogInformation("Loaded gear template: {TripType} from {File}",
                                template.TripType, Path.GetFileName(file));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load gear template from file: {File}", file);
                    }
                }

                _logger.LogInformation("Loaded {Count} gear templates", _templates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load gear templates");
            }
        }

        public BaseGearTemplate GetTemplateByTripType(string tripType)
        {
            var normalizedTripType = tripType.ToLower();

            if (_templates.TryGetValue(normalizedTripType, out var template))
            {
                return template;
            }

            // Default fallback - try to return backpacking if available
            if (_templates.TryGetValue("backpacking", out var defaultTemplate))
            {
                _logger.LogWarning("Trip type '{TripType}' not found, using backpacking template as fallback", tripType);
                return defaultTemplate;
            }

            // If no templates loaded, return empty template
            _logger.LogError("No gear templates available for trip type: {TripType}", tripType);
            return new BaseGearTemplate
            {
                TripType = tripType,
                Categories = new List<GearCategory>()
            };
        }

        public IEnumerable<BaseGearTemplate> GetAllTemplates()
        {
            return _templates.Values;
        }
    }
}