# Week 11: Equipment System & Room Navigation

> **Template Purpose:** This template represents a working solution through Week 10. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## How to Use This Template

### Option A: Continue Your Own Repository (Recommended)
If you're caught up from Week 10:
1. **DO NOT** clone this template
2. Continue working in your existing repository
3. Follow this README to add Equipment and Rooms to YOUR project

### Option B: Fresh Start (If Behind)
If you've fallen behind:
1. Accept this GitHub Classroom assignment
2. This becomes your new "main" repository
3. Complete this week's tasks in the template

> **Critical:** Room Navigation learned this week is DIRECTLY used in the Final Exam. Make sure you understand it!

---

## Overview

This week you'll implement TWO major features:

1. **Equipment System** - Weapons and Armor that affect combat
2. **Room Navigation** - A navigable world with directional exits (N/S/E/W)

Both features involve creating new entities, configuring EF Core relationships, and learning important patterns you'll use in the final project.

## Learning Objectives

By completing this assignment, you will:
- [ ] Create an Equipment entity with Weapon and Armor properties
- [ ] Create a Room entity with directional navigation (N/S/E/W exits)
- [ ] Configure **self-referencing relationships** (Room → Room)
- [ ] Configure one-to-one and one-to-many relationships
- [ ] Implement basic room navigation in the game
- [ ] Generate and apply migrations for new entities

## Prerequisites

Before starting, ensure you have:
- [ ] Completed Week 10 assignment (or are using this template)
- [ ] Working TPH inheritance for Characters and Abilities
- [ ] Understanding of EF Core relationships
- [ ] Successful migrations experience

## What's New This Week

| Concept | Description |
|---------|-------------|
| Equipment Entity | Links weapons and armor to players |
| Item Entity | Base class for weapons and armor |
| **Room Entity** | Represents a location in the game world |
| **Self-Referencing FK** | Room.NorthRoomId points to another Room |
| **Directional Navigation** | N/S/E/W movement between rooms |
| One-to-One Relationship | Player has one Equipment |
| One-to-Many Relationship | Room has many Players/Monsters |

---

## Assignment Tasks

### Task 1: Create the Equipment Class

**What to do:**
- Create an `Equipment` class that holds weapon and armor references
- This is the "equipment slot" for a player

**Equipment.cs:**
```csharp
public class Equipment
{
    public int Id { get; set; }

    // Foreign keys
    public int? WeaponId { get; set; }
    public int? ArmorId { get; set; }

    // Navigation properties
    public virtual Item Weapon { get; set; }
    public virtual Item Armor { get; set; }

    // Reference back to player
    public virtual Player Player { get; set; }
}
```

### Task 2: Create the Item Class

**What to do:**
- Create an `Item` class for weapons and armor
- Items have attack/defense values that modify combat

**Item.cs:**
```csharp
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }  // "Weapon" or "Armor"
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Weight { get; set; }
    public int Value { get; set; }
}
```

### Task 3: Update the Player Class

**What to do:**
- Add an Equipment property to Player
- Create helper methods to get total attack/defense

**Player.cs:**
```csharp
public class Player : Character
{
    public int? EquipmentId { get; set; }
    public virtual Equipment Equipment { get; set; }

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
```

### Task 4: Update GameContext

**What to do:**
- Add `DbSet` properties for Equipment and Items
- Configure relationships

**GameContext.cs:**
```csharp
public class GameContext : DbContext
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Ability> Abilities { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Existing TPH configuration...

        // Equipment-Item relationships
        modelBuilder.Entity<Equipment>()
            .HasOne(e => e.Weapon)
            .WithMany()
            .HasForeignKey(e => e.WeaponId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Equipment>()
            .HasOne(e => e.Armor)
            .WithMany()
            .HasForeignKey(e => e.ArmorId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
```

### Task 5: Update GameEngine for Combat

**What to do:**
- Modify the attack method to use equipment bonuses
- Display equipment information

**GameEngine.cs:**
```csharp
public void AttackCharacter(Player player, Character target)
{
    int attackPower = player.GetTotalAttack();
    int defense = 0;

    if (target is Player targetPlayer)
    {
        defense = targetPlayer.GetTotalDefense();
    }

    int damage = Math.Max(0, attackPower - defense);
    target.HitPoints -= damage;

    Console.WriteLine($"{player.Name} attacks {target.Name} for {damage} damage!");

    if (player.Equipment?.Weapon != null)
    {
        Console.WriteLine($"  (Using {player.Equipment.Weapon.Name})");
    }
}
```

### Task 6: Create the Room Class

**What to do:**
- Create a `Room` class with directional exits
- This uses **self-referencing foreign keys** - a Room points to other Rooms!

**Room.cs:**
```csharp
public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Self-referencing foreign keys (nullable - not all rooms have all exits)
    public int? NorthRoomId { get; set; }
    public int? SouthRoomId { get; set; }
    public int? EastRoomId { get; set; }
    public int? WestRoomId { get; set; }

    // Navigation properties for exits
    public virtual Room NorthRoom { get; set; }
    public virtual Room SouthRoom { get; set; }
    public virtual Room EastRoom { get; set; }
    public virtual Room WestRoom { get; set; }

    // Entities in this room
    public virtual ICollection<Player> Players { get; set; }
    public virtual ICollection<Monster> Monsters { get; set; }
}
```

### Task 7: Update Player and Monster with RoomId

**What to do:**
- Add `RoomId` to Player and Monster so they can be "in" a room

**Player.cs (add these properties):**
```csharp
public int? RoomId { get; set; }
public virtual Room Room { get; set; }
```

**Monster.cs (add these properties):**
```csharp
public int? RoomId { get; set; }
public virtual Room Room { get; set; }
```

### Task 8: Configure Room Relationships in GameContext

**What to do:**
- Configure the self-referencing relationships for Room exits
- Configure Player/Monster → Room relationships

**GameContext.cs (add to OnModelCreating):**
```csharp
// Self-referencing relationships for Room navigation
// Each direction points to another Room (or null if no exit)
modelBuilder.Entity<Room>()
    .HasOne(r => r.NorthRoom)
    .WithMany()
    .HasForeignKey(r => r.NorthRoomId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Room>()
    .HasOne(r => r.SouthRoom)
    .WithMany()
    .HasForeignKey(r => r.SouthRoomId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Room>()
    .HasOne(r => r.EastRoom)
    .WithMany()
    .HasForeignKey(r => r.EastRoomId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Room>()
    .HasOne(r => r.WestRoom)
    .WithMany()
    .HasForeignKey(r => r.WestRoomId)
    .OnDelete(DeleteBehavior.Restrict);

// Player and Monster location relationships
modelBuilder.Entity<Player>()
    .HasOne(p => p.Room)
    .WithMany(r => r.Players)
    .HasForeignKey(p => p.RoomId)
    .OnDelete(DeleteBehavior.SetNull);

modelBuilder.Entity<Monster>()
    .HasOne(m => m.Room)
    .WithMany(r => r.Monsters)
    .HasForeignKey(m => m.RoomId)
    .OnDelete(DeleteBehavior.SetNull);
```

### Task 9: Implement Room Navigation in GameEngine

**What to do:**
- Add menu options for navigation
- Implement the movement logic

**GameEngine.cs:**
```csharp
public void MovePlayer(Player player, string direction)
{
    var currentRoom = _context.Rooms
        .Include(r => r.NorthRoom)
        .Include(r => r.SouthRoom)
        .Include(r => r.EastRoom)
        .Include(r => r.WestRoom)
        .FirstOrDefault(r => r.Id == player.RoomId);

    if (currentRoom == null)
    {
        Console.WriteLine("You are not in any room!");
        return;
    }

    Room nextRoom = direction.ToUpper() switch
    {
        "N" or "NORTH" => currentRoom.NorthRoom,
        "S" or "SOUTH" => currentRoom.SouthRoom,
        "E" or "EAST" => currentRoom.EastRoom,
        "W" or "WEST" => currentRoom.WestRoom,
        _ => null
    };

    if (nextRoom == null)
    {
        Console.WriteLine("You cannot go that way!");
        return;
    }

    player.RoomId = nextRoom.Id;
    _context.SaveChanges();

    Console.WriteLine($"You move {direction} to {nextRoom.Name}.");
    Console.WriteLine(nextRoom.Description);
}

public void DisplayCurrentRoom(Player player)
{
    var room = _context.Rooms
        .Include(r => r.Players)
        .Include(r => r.Monsters)
        .FirstOrDefault(r => r.Id == player.RoomId);

    if (room == null)
    {
        Console.WriteLine("You are not in any room.");
        return;
    }

    Console.WriteLine($"\n=== {room.Name} ===");
    Console.WriteLine(room.Description);

    // Show exits
    var exits = new List<string>();
    if (room.NorthRoomId.HasValue) exits.Add("North");
    if (room.SouthRoomId.HasValue) exits.Add("South");
    if (room.EastRoomId.HasValue) exits.Add("East");
    if (room.WestRoomId.HasValue) exits.Add("West");
    Console.WriteLine($"Exits: {string.Join(", ", exits)}");

    // Show monsters
    if (room.Monsters.Any())
    {
        Console.WriteLine($"Monsters here: {string.Join(", ", room.Monsters.Select(m => m.Name))}");
    }
}
```

### Task 10: Generate and Apply Migrations

**Commands:**
```bash
# Add Equipment, Items, and Rooms tables
dotnet ef migrations add AddEquipmentAndRooms --project ConsoleRpgEntities

# Apply migration
dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg
```

---

## Stretch Goal (+10%)

**Implement Item Hierarchy with TPH**

Create an abstract `Item` class with `Weapon` and `Armor` as derived types:

```csharp
public abstract class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public int Durability { get; set; }

    public abstract void Use(Player player);
}

public class Weapon : Item
{
    public int AttackPower { get; set; }

    public override void Use(Player player)
    {
        Console.WriteLine($"{player.Name} equips {Name}!");
    }
}

public class Armor : Item
{
    public int DefenseRating { get; set; }

    public override void Use(Player player)
    {
        Console.WriteLine($"{player.Name} puts on {Name}!");
    }
}
```

Then configure TPH in `GameContext`:

```csharp
modelBuilder.Entity<Item>()
    .HasDiscriminator<string>("ItemType")
    .HasValue<Weapon>("Weapon")
    .HasValue<Armor>("Armor");
```

---

## Project Structure

This template uses a **two-project architecture**:

```
ConsoleRpgFinal.sln
│
├── ConsoleRpg/                        # UI & Game Logic
│   ├── Program.cs
│   └── GameEngine.cs                  # UPDATE: Add combat & navigation
│
└── ConsoleRpgEntities/                # Data & Models
    ├── Models/
    │   ├── Characters/
    │   │   ├── Player.cs              # UPDATE: Add RoomId, EquipmentId
    │   │   └── Monsters/
    │   │       └── Monster.cs         # UPDATE: Add RoomId
    │   ├── Equipments/                # CREATE this folder
    │   │   ├── Equipment.cs           # CREATE: Equipment slot class
    │   │   └── Item.cs                # CREATE: Weapon/Armor data
    │   └── Rooms/                     # CREATE this folder
    │       └── Room.cs                # CREATE: Room with N/S/E/W exits
    └── Data/
        └── GameContext.cs             # UPDATE: Add DbSets & relationships
```

> **Note:** Items marked CREATE are what you'll build. Items marked UPDATE are existing files you'll modify.

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| Equipment Class | 15 | Proper Equipment entity with relationships |
| Item Class | 15 | Item entity with attack/defense properties |
| **Room Class** | 15 | Room entity with N/S/E/W exits |
| Player/Monster Integration | 15 | RoomId, EquipmentId properties added |
| **GameContext Setup** | 15 | All relationships configured correctly |
| **Room Navigation** | 15 | MovePlayer and DisplayCurrentRoom work |
| Code Quality | 10 | Clean, readable, follows SOLID |
| **Total** | **100** | |
| **Stretch: Item TPH** | **+10** | Abstract Item with Weapon/Armor hierarchy |

---

## How This Connects to the Final Project

- **Room Navigation** is directly used in the final - same N/S/E/W pattern!
- **Self-referencing relationships** appear in the final's world map
- **Equipment system** is central to the RPG gameplay
- Item management becomes part of the inventory system (Week 12)
- These patterns prepare you for the final project's complexity

> **Important:** The Room Navigation you build this week is EXACTLY what you'll use in the final exam. Master it now!

---

## Database Diagram

```
                              ┌─────────────┐
                         ┌───▶│    Room     │◀───┐
                         │    ├─────────────┤    │
                         │    │ Id          │    │
                         │    │ Name        │    │
                         │    │ Description │    │
                         │    │ NorthRoomId │────┤ (Self-referencing!)
┌─────────────┐          │    │ SouthRoomId │────┤
│   Player    │──────────┘    │ EastRoomId  │────┤
├─────────────┤               │ WestRoomId  │────┘
│ Id          │               └─────────────┘
│ Name        │                     ▲
│ RoomId      │─────────────────────┘
│ EquipmentId │────┐
│ ...         │    │
└─────────────┘    │     ┌─────────────┐     ┌─────────────┐
                   └────▶│  Equipment  │────▶│    Item     │
                         ├─────────────┤     ├─────────────┤
                         │ Id          │     │ Id          │
                         │ WeaponId    │────▶│ Name        │
                         │ ArmorId     │────▶│ Type        │
                         │ ...         │     │ Attack      │
                         └─────────────┘     │ Defense     │
                                             │ ...         │
                                             └─────────────┘
```

---

## Tips

- Use nullable foreign keys (`int?`) for optional relationships
- Remember to include related entities with `.Include()` when querying
- Test equipment bonuses with different weapon/armor combinations
- Use the `??` null-coalescing operator for default values

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [EF Core One-to-One Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one)
- [EF Core Navigation Properties](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations)
- [Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examples
