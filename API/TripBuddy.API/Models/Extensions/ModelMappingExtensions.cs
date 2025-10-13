using TripBuddy.API.Models.Database;

namespace TripBuddy.API.Models.Extensions;

/// <summary>
/// Extension methods for mapping between database entities and DTOs
/// </summary>
public static class ModelMappingExtensions
{
    /// <summary>
    /// Convert Park entity to ParkDto
    /// </summary>
    public static ParkDto ToDto(this Park park, bool includeThingsToDo = false)
    {
        var dto = new ParkDto
        {
            Id = park.Id,
            Name = park.Name,
            Description = park.Description,
            FullDescription = park.FullDescription,
            NpsParkCode = park.NpsParkCode,
            NpsDesignation = park.NpsDesignation,
            Location = park.Location,
            StreetAddress = park.StreetAddress,
            City = park.City,
            StateCode = park.StateCode,
            PostalCode = park.PostalCode,
            Latitude = park.Latitude,
            Longitude = park.Longitude,
            Phone = park.Phone,
            Email = park.Email,
            WebsiteUrl = park.WebsiteUrl,
            ParkType = park.ParkType,
            IsActive = park.IsActive
        };

        if (includeThingsToDo && park.ThingsToDo?.Any() == true)
        {
            dto.ThingsToDo = park.ThingsToDo.Select(t => t.ToDto()).ToList();
        }

        return dto;
    }

    /// <summary>
    /// Convert ParkThingToDo entity to ParkThingToDoDto
    /// </summary>
    public static ParkThingToDoDto ToDto(this ParkThingToDo thingToDo, bool includePark = false)
    {
        var dto = new ParkThingToDoDto
        {
            Id = thingToDo.Id,
            ParkId = thingToDo.ParkId,
            NpsThingId = thingToDo.NpsThingId,
            Title = thingToDo.Title,
            ShortDescription = thingToDo.ShortDescription,
            FullDescription = thingToDo.FullDescription,
            Duration = thingToDo.Duration,
            Season = thingToDo.Season,
            ActivityTags = thingToDo.ActivityTags,
            AccessibilityInfo = thingToDo.AccessibilityInfo,
            FeeInfo = thingToDo.FeeInfo,
            LocationDescription = thingToDo.LocationDescription,
            Url = thingToDo.Url,
            IsActive = thingToDo.IsActive
        };

        if (includePark && thingToDo.Park != null)
        {
            dto.Park = thingToDo.Park.ToSummaryDto();
        }

        return dto;
    }

    /// <summary>
    /// Convert Park entity to lightweight ParkSummaryDto
    /// </summary>
    public static ParkSummaryDto ToSummaryDto(this Park park)
    {
        return new ParkSummaryDto
        {
            Id = park.Id,
            Name = park.Name,
            NpsParkCode = park.NpsParkCode,
            Location = park.Location,
            StateCode = park.StateCode
        };
    }

    /// <summary>
    /// Convert collection of Park entities to DTOs
    /// </summary>
    public static List<ParkDto> ToDtoList(this IEnumerable<Park> parks, bool includeThingsToDo = false)
    {
        return parks.Select(p => p.ToDto(includeThingsToDo)).ToList();
    }

    /// <summary>
    /// Convert collection of ParkThingToDo entities to DTOs
    /// </summary>
    public static List<ParkThingToDoDto> ToDtoList(this IEnumerable<ParkThingToDo> thingsToDo, bool includePark = false)
    {
        return thingsToDo.Select(t => t.ToDto(includePark)).ToList();
    }
}