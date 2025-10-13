using TripBuddy.API.Models;
using TripBuddy.API.Models.Database;
using TripBuddy.API.Models.Extensions;
using TripBuddy.API.Services.Data;

namespace TripBuddy.API.Services.Business;

/// <summary>
/// Business service for park-related operations
/// Orchestrates between repositories and handles business logic
/// </summary>
public class ParkService : IParkService
{
    private readonly IParkRepository _parkRepository;
    private readonly IThingsToDoRepository _thingsToDoRepository;

    public ParkService(IParkRepository parkRepository, IThingsToDoRepository thingsToDoRepository)
    {
        _parkRepository = parkRepository;
        _thingsToDoRepository = thingsToDoRepository;
    }

    public async Task<IEnumerable<ParkDto>> GetAllParksAsync(bool includeThingsToDo = false)
    {
        var parks = await _parkRepository.GetAllAsync();

        if (includeThingsToDo)
        {
            var parksWithActivities = new List<ParkDto>();
            foreach (var park in parks)
            {
                var activities = await _thingsToDoRepository.GetByParkIdAsync(park.Id);
                var parkEntity = park;
                parkEntity.ThingsToDo = activities.ToList();
                parksWithActivities.Add(parkEntity.ToDto(includeThingsToDo: true));
            }
            return parksWithActivities;
        }

        return parks.ToDtoList();
    }

    public async Task<ParkDto?> GetParkByIdAsync(int id, bool includeThingsToDo = false)
    {
        var park = await _parkRepository.GetByIdAsync(id);
        if (park == null)
            return null;

        if (includeThingsToDo)
        {
            var activities = await _thingsToDoRepository.GetByParkIdAsync(id);
            park.ThingsToDo = activities.ToList();
        }

        return park.ToDto(includeThingsToDo);
    }

    public async Task<ParkDto?> GetParkByNpsCodeAsync(string npsCode, bool includeThingsToDo = false)
    {
        var park = await _parkRepository.GetByNpsCodeAsync(npsCode);
        if (park == null)
            return null;

        if (includeThingsToDo)
        {
            var activities = await _thingsToDoRepository.GetByParkIdAsync(park.Id);
            park.ThingsToDo = activities.ToList();
        }

        return park.ToDto(includeThingsToDo);
    }

    public async Task<IEnumerable<ParkDto>> GetParksByStateAsync(string stateCode)
    {
        var parks = await _parkRepository.GetByStateAsync(stateCode);
        return parks.ToDtoList();
    }

    public async Task<IEnumerable<ParkDto>> SearchParksAsync(string searchTerm)
    {
        var parks = await _parkRepository.SearchAsync(searchTerm);
        return parks.ToDtoList();
    }

    public async Task<IEnumerable<ParkThingToDoDto>> GetParkActivitiesAsync(int parkId)
    {
        var activities = await _thingsToDoRepository.GetByParkIdAsync(parkId);
        return activities.ToDtoList();
    }

    public async Task<IEnumerable<ParkThingToDoDto>> SearchActivitiesAsync(string searchTerm, int? parkId = null)
    {
        var activities = await _thingsToDoRepository.SearchAsync(searchTerm);

        if (parkId.HasValue)
        {
            activities = activities.Where(a => a.ParkId == parkId.Value);
        }

        return activities.ToDtoList();
    }

    public async Task<ParkDto> CreateParkAsync(ParkDto parkDto)
    {
        // Convert DTO to entity
        var park = new Park
        {
            Name = parkDto.Name,
            Description = parkDto.Description,
            FullDescription = parkDto.FullDescription,
            NpsParkCode = parkDto.NpsParkCode,
            NpsDesignation = parkDto.NpsDesignation,
            Location = parkDto.Location,
            StreetAddress = parkDto.StreetAddress,
            City = parkDto.City,
            StateCode = parkDto.StateCode,
            PostalCode = parkDto.PostalCode,
            Latitude = parkDto.Latitude,
            Longitude = parkDto.Longitude,
            Phone = parkDto.Phone,
            Email = parkDto.Email,
            WebsiteUrl = parkDto.WebsiteUrl,
            ParkType = parkDto.ParkType,
            IsActive = parkDto.IsActive
        };

        var createdPark = await _parkRepository.CreateAsync(park);
        return createdPark.ToDto();
    }

    public async Task<ParkDto?> UpdateParkAsync(int id, ParkDto parkDto)
    {
        var existingPark = await _parkRepository.GetByIdAsync(id);
        if (existingPark == null)
            return null;

        // Update entity with DTO values
        existingPark.Name = parkDto.Name;
        existingPark.Description = parkDto.Description;
        existingPark.FullDescription = parkDto.FullDescription;
        existingPark.Location = parkDto.Location;
        existingPark.StreetAddress = parkDto.StreetAddress;
        existingPark.City = parkDto.City;
        existingPark.StateCode = parkDto.StateCode;
        existingPark.PostalCode = parkDto.PostalCode;
        existingPark.Latitude = parkDto.Latitude;
        existingPark.Longitude = parkDto.Longitude;
        existingPark.Phone = parkDto.Phone;
        existingPark.Email = parkDto.Email;
        existingPark.WebsiteUrl = parkDto.WebsiteUrl;
        existingPark.IsActive = parkDto.IsActive;

        var updatedPark = await _parkRepository.UpdateAsync(existingPark);
        return updatedPark?.ToDto();
    }

    public async Task<bool> DeleteParkAsync(int id)
    {
        return await _parkRepository.DeleteAsync(id);
    }
}