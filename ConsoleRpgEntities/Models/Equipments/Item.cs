namespace ConsoleRpgEntities.Models.Equipments;

/// <summary>
/// Represents an item that can be equipped as a weapon or armor.
/// Items have attack and defense values that modify combat.
/// </summary>
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Weapon" or "Armor"
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Weight { get; set; }
    public int Value { get; set; }
}
