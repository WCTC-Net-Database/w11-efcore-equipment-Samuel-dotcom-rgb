using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Equipments;
using ConsoleRpgEntities.Models.Rooms;

namespace ConsoleRpgEntities.Models.Characters
{
    public class Player : ITargetable, IPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Health { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }

        // ===========================================
        // ROOM LOCATION
        // ===========================================
        public int? RoomId { get; set; }
        public virtual Room? Room { get; set; }

        // ===========================================
        // EQUIPMENT (One-to-One)
        // ===========================================
        public int? EquipmentId { get; set; }
        public virtual Equipment? Equipment { get; set; }

        // Navigation property for abilities (many-to-many)
        public virtual ICollection<Ability> Abilities { get; set; } = new List<Ability>();

        public void Attack(ITargetable target)
        {
            Console.WriteLine($"{Name} attacks {target.Name} with a sword!");
        }

        public void UseAbility(IAbility ability, ITargetable target)
        {
            if (Abilities.Contains(ability))
            {
                ability.Activate(this, target);
            }
            else
            {
                Console.WriteLine($"{Name} does not have the ability {ability.Name}!");
            }
        }

        // ===========================================
        // EQUIPMENT COMBAT HELPERS
        // ===========================================
        public int GetTotalAttack()
        {
            int baseAttack = Level * 2;
            int weaponBonus = Equipment?.Weapon?.Attack ?? 0;
            return baseAttack + weaponBonus;
        }

        public int GetTotalDefense()
        {
            int baseDefense = Level;
            int armorBonus = Equipment?.Armor?.Defense ?? 0;
            return baseDefense + armorBonus;
        }
    }
}
