using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;

namespace ConsoleRpgEntities.Models.Rooms
{
    /// <summary>
    /// Room entity with directional navigation.
    ///
    /// KEY CONCEPT: Self-Referencing Relationships
    /// A Room can point to other Rooms via NorthRoomId, SouthRoomId, etc.
    /// This is called a "self-referencing" foreign key - the table references itself.
    ///
    /// Example: If Room 1 (Town Square) has NorthRoomId = 2 (Market),
    /// then going North from Town Square takes you to the Market.
    /// </summary>
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // ===========================================
        // DIRECTIONAL EXIT FOREIGN KEYS
        // ===========================================
        // These are nullable because a room may not have exits in all directions.
        // For example, a corner room might only have South and East exits.

        public int? NorthRoomId { get; set; }
        public int? SouthRoomId { get; set; }
        public int? EastRoomId { get; set; }
        public int? WestRoomId { get; set; }

        // ===========================================
        // NAVIGATION PROPERTIES FOR EXITS
        // ===========================================
        // These let you access the actual Room objects, not just their IDs.
        // Example: room.NorthRoom?.Name gives you the name of the room to the north.

        public virtual Room? NorthRoom { get; set; }
        public virtual Room? SouthRoom { get; set; }
        public virtual Room? EastRoom { get; set; }
        public virtual Room? WestRoom { get; set; }

        // ===========================================
        // ENTITIES IN THIS ROOM
        // ===========================================
        // One-to-many: A room can contain many players and monsters.

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<Monster> Monsters { get; set; } = new List<Monster>();

        /// <summary>
        /// Helper method to check if a direction has an exit.
        /// </summary>
        public bool HasExit(string direction)
        {
            return direction.ToUpper() switch
            {
                "N" or "NORTH" => NorthRoomId.HasValue,
                "S" or "SOUTH" => SouthRoomId.HasValue,
                "E" or "EAST" => EastRoomId.HasValue,
                "W" or "WEST" => WestRoomId.HasValue,
                _ => false
            };
        }

        /// <summary>
        /// Helper method to get the room ID in a given direction.
        /// Returns null if no exit exists.
        /// </summary>
        public int? GetExitRoomId(string direction)
        {
            return direction.ToUpper() switch
            {
                "N" or "NORTH" => NorthRoomId,
                "S" or "SOUTH" => SouthRoomId,
                "E" or "EAST" => EastRoomId,
                "W" or "WEST" => WestRoomId,
                _ => null
            };
        }
    }
}
