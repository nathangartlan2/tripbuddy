using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace TripBuddy.API.Models.Database;

/// <summary>
/// Represents things to do at each park from the NPS thingstodo endpoint
/// </summary>
[Table("park_things_to_do")]
public class ParkThingToDo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("park_id")]
    public int ParkId { get; set; }

    [MaxLength(50)]
    [Column("nps_thing_id")]
    public string? NpsThingId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("short_description")]
    public string? ShortDescription { get; set; }

    [Column("full_description")]
    public string? FullDescription { get; set; }

    [MaxLength(100)]
    [Column("duration")]
    public string? Duration { get; set; }

    [MaxLength(100)]
    [Column("season")]
    public string? Season { get; set; }

    [Column("activity_tags")]
    public string[]? ActivityTags { get; set; }

    [Column("accessibility_info")]
    public string? AccessibilityInfo { get; set; }

    [Column("fee_info")]
    public string? FeeInfo { get; set; }

    [Column("location_description")]
    public string? LocationDescription { get; set; }

    [Column("url")]
    public string? Url { get; set; }

    // Full-text search optimization
    [Column("search_vector")]
    public NpgsqlTsVector? SearchVector { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ParkId")]
    public virtual Park Park { get; set; } = null!;
}