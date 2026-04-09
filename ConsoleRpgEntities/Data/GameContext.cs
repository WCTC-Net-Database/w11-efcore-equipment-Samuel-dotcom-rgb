using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using ConsoleRpgEntities.Models.Equipments;
using ConsoleRpgEntities.Models.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ConsoleRpgEntities.Data
{
    public class GameContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Monster> Monsters { get; set; }
        public DbSet<Ability> Abilities { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Item> Items { get; set; }

        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===========================================
            // TPH CONFIGURATION (from Week 10)
            // ===========================================

            // Configure TPH for Monster hierarchy
            modelBuilder.Entity<Monster>()
                .HasDiscriminator<string>(m => m.MonsterType)
                .HasValue<Goblin>("Goblin");

            // Configure TPH for Ability hierarchy
            modelBuilder.Entity<Ability>()
                .HasDiscriminator<string>(pa => pa.AbilityType)
                .HasValue<ShoveAbility>("ShoveAbility");

            // Configure many-to-many: Player <-> Ability
            modelBuilder.Entity<Player>()
                .HasMany(p => p.Abilities)
                .WithMany(a => a.Players)
                .UsingEntity(j => j.ToTable("PlayerAbilities"));

            // ===========================================
            // EQUIPMENT RELATIONSHIPS
            // ===========================================
            // Equipment has two optional Item references (weapon and armor).
            // We use .OnDelete(DeleteBehavior.Restrict) to prevent cascade issues
            // when an Item is referenced by multiple Equipment slots.

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

            // ===========================================
            // ROOM NAVIGATION (Self-Referencing Relationships)
            // ===========================================
            // KEY CONCEPT: A Room table that references itself!
            // Each Room can point to other Rooms via NorthRoomId, SouthRoomId, etc.
            //
            // We use .OnDelete(DeleteBehavior.Restrict) to prevent cascade issues.
            // Without this, deleting one room could cascade-delete connected rooms.

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

            // ===========================================
            // PLAYER/MONSTER -> ROOM RELATIONSHIPS
            // ===========================================

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

            base.OnModelCreating(modelBuilder);
        }
    }
}


