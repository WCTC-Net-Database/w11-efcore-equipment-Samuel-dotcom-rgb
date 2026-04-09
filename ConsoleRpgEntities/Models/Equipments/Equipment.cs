using ConsoleRpgEntities.Models.Characters;

namespace ConsoleRpgEntities.Models.Equipments;

/// <summary>
/// Represents a player's equipment slots (weapon and armor).
/// This creates a one-to-one relationship: one Player has one Equipment.
/// Equipment references Items via foreign keys.
/// </summary>
public class Equipment
{
    public int Id { get; set; }

    // Foreign keys to Items (nullable - slot can be empty)
    public int? WeaponId { get; set; }
    public int? ArmorId { get; set; }

    // Navigation properties
    public virtual Item? Weapon { get; set; }
    public virtual Item? Armor { get; set; }

    // Reference back to player
    public virtual Player Player { get; set; } = null!;
}
