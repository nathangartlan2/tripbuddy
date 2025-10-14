using TripBuddy.API.Models;
using TripBuddy.API.Models.Database;
using TripBuddy.API.Models.Extensions;
using TripBuddy.API.Services.Data;

namespace TripBuddy.API.Services.Business;

/// <summary>
/// Business service for park things to do operations
/// Orchestrates between repositories and handles business logic
/// </summary>
public class ParkThingsToDoService : IParkThingsToDoService
{
    private readonly IThingsToDoRepository _thingsToDoRepository;
    private readonly IParkRepository _parkRepository;

    public ParkThingsToDoService(IThingsToDoRepository thingsToDoRepository, IParkRepository parkRepository)
    {
        _thingsToDoRepository = thingsToDoRepository;
        _parkRepository = parkRepository;
    }

    public async Task<IEnumerable<ParkThingToDoDto>> GetAllThingsToDoAsync()
    {
        var thingsToDo = await _thingsToDoRepository.GetAllAsync();
        return thingsToDo.ToDtoList();
    }

    public async Task<ParkThingToDoDto?> GetThingToDoByIdAsync(int id, bool includePark = false)
    {
        var thingToDo = await _thingsToDoRepository.GetByIdAsync(id);
        if (thingToDo == null)
            return null;

        return thingToDo.ToDto(includePark);
    }

    public async Task<IEnumerable<ParkThingToDoDto>> GetThingsToDoByParkIdAsync(int parkId)
    {
        var thingsToDo = await _thingsToDoRepository.GetByParkIdAsync(parkId);
        return thingsToDo.ToDtoList();
    }

    public async Task<IEnumerable<ParkThingToDoDto>> SearchThingsToDoAsync(string searchTerm, int? parkId = null)
    {
        var thingsToDo = await _thingsToDoRepository.SearchAsync(searchTerm);

        if (parkId.HasValue)
        {
            thingsToDo = thingsToDo.Where(t => t.ParkId == parkId.Value);
        }

        return thingsToDo.ToDtoList();
    }

    public async Task<IEnumerable<ParkThingToDoDto>> GetThingsToDoBySeasonAsync(string season, int? parkId = null)
    {
        var thingsToDo = await _thingsToDoRepository.GetBySeasonAsync(season);

        if (parkId.HasValue)
        {
            thingsToDo = thingsToDo.Where(t => t.ParkId == parkId.Value);
        }

        return thingsToDo.ToDtoList();
    }

    public async Task<IEnumerable<ParkThingToDoDto>> GetThingsToDoByActivityTagsAsync(string[] activityTags, int? parkId = null)
    {
        var thingsToDo = await _thingsToDoRepository.GetByActivityTagsAsync(activityTags);

        if (parkId.HasValue)
        {
            thingsToDo = thingsToDo.Where(t => t.ParkId == parkId.Value);
        }

        return thingsToDo.ToDtoList();
    }

    public async Task<ParkThingToDoDto> CreateThingToDoAsync(ParkThingToDoDto thingToDoDto)
    {
        // Validate park exists
        var park = await _parkRepository.GetByIdAsync(thingToDoDto.ParkId);
        if (park == null)
        {
            throw new ArgumentException($"Park with ID {thingToDoDto.ParkId} not found", nameof(thingToDoDto));
        }

        // Convert DTO to entity
        var thingToDo = new ParkThingToDo
        {
            ParkId = thingToDoDto.ParkId,
            NpsThingId = thingToDoDto.NpsThingId,
            Title = thingToDoDto.Title,
            ShortDescription = thingToDoDto.ShortDescription,
            FullDescription = thingToDoDto.FullDescription,
            Duration = thingToDoDto.Duration,
            Season = thingToDoDto.Season,
            ActivityTags = thingToDoDto.ActivityTags,
            AccessibilityInfo = thingToDoDto.AccessibilityInfo,
            FeeInfo = thingToDoDto.FeeInfo,
            LocationDescription = thingToDoDto.LocationDescription,
            Url = thingToDoDto.Url,
            IsActive = thingToDoDto.IsActive
        };

        var createdThingToDo = await _thingsToDoRepository.CreateAsync(thingToDo);
        return createdThingToDo.ToDto();
    }

    public async Task<ParkThingToDoDto?> UpdateThingToDoAsync(int id, ParkThingToDoDto thingToDoDto)
    {
        var existingThingToDo = await _thingsToDoRepository.GetByIdAsync(id);
        if (existingThingToDo == null)
            return null;

        // Validate park exists if park ID is being changed
        if (existingThingToDo.ParkId != thingToDoDto.ParkId)
        {
            var park = await _parkRepository.GetByIdAsync(thingToDoDto.ParkId);
            if (park == null)
            {
                throw new ArgumentException($"Park with ID {thingToDoDto.ParkId} not found", nameof(thingToDoDto));
            }
        }

        // Update entity with DTO values
        existingThingToDo.ParkId = thingToDoDto.ParkId;
        existingThingToDo.NpsThingId = thingToDoDto.NpsThingId;
        existingThingToDo.Title = thingToDoDto.Title;
        existingThingToDo.ShortDescription = thingToDoDto.ShortDescription;
        existingThingToDo.FullDescription = thingToDoDto.FullDescription;
        existingThingToDo.Duration = thingToDoDto.Duration;
        existingThingToDo.Season = thingToDoDto.Season;
        existingThingToDo.ActivityTags = thingToDoDto.ActivityTags;
        existingThingToDo.AccessibilityInfo = thingToDoDto.AccessibilityInfo;
        existingThingToDo.FeeInfo = thingToDoDto.FeeInfo;
        existingThingToDo.LocationDescription = thingToDoDto.LocationDescription;
        existingThingToDo.Url = thingToDoDto.Url;
        existingThingToDo.IsActive = thingToDoDto.IsActive;

        var updatedThingToDo = await _thingsToDoRepository.UpdateAsync(existingThingToDo);
        return updatedThingToDo?.ToDto();
    }

    public async Task<bool> DeleteThingToDoAsync(int id)
    {
        return await _thingsToDoRepository.DeleteAsync(id);
    }
}