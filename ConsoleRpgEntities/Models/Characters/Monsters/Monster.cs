using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Rooms;

namespace ConsoleRpgEntities.Models.Characters.Monsters
{
    public abstract class Monster : IMonster, ITargetable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Health { get; set; }
        public int AggressionLevel { get; set; }
        public string MonsterType { get; set; } = string.Empty;

        // ===========================================
        // ROOM LOCATION
        // ===========================================
        public int? RoomId { get; set; }
        public virtual Room? Room { get; set; }

        protected Monster()
        {
        }

        public abstract void Attack(ITargetable target);
    }
}
