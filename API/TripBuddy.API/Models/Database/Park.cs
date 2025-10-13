using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripBuddy.API.Models.Database;

/// <summary>
/// Represents a park entity from the NPS API with enhanced location and contact information
/// </summary>
[Table("parks")]
public class Park
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    // Core park information
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("full_description")]
    public string? FullDescription { get; set; }

    // NPS-specific identifiers
    [MaxLength(10)]
    [Column("nps_park_code")]
    public string? NpsParkCode { get; set; }

    [MaxLength(100)]
    [Column("nps_designation")]
    public string? NpsDesignation { get; set; }

    // Location information
    [MaxLength(255)]
    [Column("location")]
    public string? Location { get; set; }

    [Column("street_address")]
    public string? StreetAddress { get; set; }

    [MaxLength(100)]
    [Column("city")]
    public string? City { get; set; }

    [MaxLength(2)]
    [Column("state_code")]
    public string? StateCode { get; set; }

    [MaxLength(10)]
    [Column("postal_code")]
    public string? PostalCode { get; set; }

    [Column("latitude", TypeName = "decimal(10,8)")]
    public decimal? Latitude { get; set; }

    [Column("longitude", TypeName = "decimal(11,8)")]
    public decimal? Longitude { get; set; }

    // Contact information
    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [Column("website_url")]
    public string? WebsiteUrl { get; set; }

    // Legacy fields (keeping for compatibility)
    [MaxLength(100)]
    [Column("park_type")]
    public string? ParkType { get; set; }

    // Metadata
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<ParkThingToDo> ThingsToDo { get; set; } = new List<ParkThingToDo>();
}