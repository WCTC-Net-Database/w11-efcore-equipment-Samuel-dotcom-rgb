using ConsoleRpg.Helpers;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using ConsoleRpgEntities.Models.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ConsoleRpg.Services;

public class GameEngine
{
    private readonly GameContext _context;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private IPlayer? _player;
    private IMonster? _goblin;

    public GameEngine(GameContext context, MenuManager menuManager, OutputManager outputManager)
    {
        _menuManager = menuManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void GameLoop()
    {
        _outputManager.Clear();

        while (true)
        {
            if (_player is Player player)
            {
                DisplayCurrentRoom(player);
            }

            _outputManager.WriteLine("\nChoose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Attack");
            _outputManager.WriteLine("2. Move North");
            _outputManager.WriteLine("3. Move South");
            _outputManager.WriteLine("4. Move East");
            _outputManager.WriteLine("5. Move West");
            _outputManager.WriteLine("6. Quit");

            _outputManager.Display();

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    AttackCharacter();
                    break;
                case "2":
                    if (_player is Player playerN) MovePlayer(playerN, "N");
                    break;
                case "3":
                    if (_player is Player playerS) MovePlayer(playerS, "S");
                    break;
                case "4":
                    if (_player is Player playerE) MovePlayer(playerE, "E");
                    break;
                case "5":
                    if (_player is Player playerW) MovePlayer(playerW, "W");
                    break;
                case "6":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose 1-6.", ConsoleColor.Red);
                    break;
            }
        }
    }

    private void AttackCharacter()
    {
        if (_player is not Player player)
            return;

        if (_goblin is not ITargetable targetableGoblin)
            return;

        int attackPower = player.GetTotalAttack();
        int defense = 0;

        if (_goblin is Player targetPlayer)
        {
            defense = targetPlayer.GetTotalDefense();
        }

        int damage = Math.Max(0, attackPower - defense);
        
        // Update the target's health based on their type
        if (_goblin is Monster goblinMonster)
        {
            goblinMonster.Health -= damage;
        }
        else if (_goblin is Player goblinPlayer)
        {
            goblinPlayer.Health -= damage;
        }

        _outputManager.WriteLine($"{player.Name} attacks {targetableGoblin.Name} for {damage} damage!", ConsoleColor.Yellow);

        if (player.Equipment?.Weapon != null)
        {
            _outputManager.WriteLine($"  (Using {player.Equipment.Weapon.Name})", ConsoleColor.Yellow);
        }

        _outputManager.Display();
        Thread.Sleep(500);
    }

    private void SetupGame()
    {
        _player = _context.Players.OfType<Player>().FirstOrDefault();
        _outputManager.WriteLine($"{_player?.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause before starting the game loop
        Thread.Sleep(500);
        GameLoop();
    }

    private void LoadMonsters()
    {
        _goblin = _context.Monsters.OfType<Goblin>().FirstOrDefault();
    }

    /// <summary>
    /// Moves the player in the specified direction (N/S/E/W) to an adjacent room.
    /// </summary>
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
            _outputManager.WriteLine("You are not in any room!", ConsoleColor.Red);
            _outputManager.Display();
            return;
        }

        Room? nextRoom = direction.ToUpper() switch
        {
            "N" or "NORTH" => currentRoom.NorthRoom,
            "S" or "SOUTH" => currentRoom.SouthRoom,
            "E" or "EAST" => currentRoom.EastRoom,
            "W" or "WEST" => currentRoom.WestRoom,
            _ => null
        };

        if (nextRoom == null)
        {
            _outputManager.WriteLine("You cannot go that way!", ConsoleColor.Red);
            _outputManager.Display();
            return;
        }

        player.RoomId = nextRoom.Id;
        _context.SaveChanges();

        _outputManager.WriteLine($"You move {direction.ToUpper()} to {nextRoom.Name}.", ConsoleColor.Green);
        _outputManager.WriteLine(nextRoom.Description, ConsoleColor.Cyan);
        _outputManager.Display();
        Thread.Sleep(500);
    }

    /// <summary>
    /// Displays the current room information including name, description, exits, and inhabitants.
    /// </summary>
    public void DisplayCurrentRoom(Player player)
    {
        var room = _context.Rooms
            .Include(r => r.Players)
            .Include(r => r.Monsters)
            .FirstOrDefault(r => r.Id == player.RoomId);

        if (room == null)
        {
            _outputManager.WriteLine("You are not in any room.", ConsoleColor.Red);
            return;
        }

        _outputManager.Clear();
        _outputManager.WriteLine($"\n=== {room.Name} ===", ConsoleColor.Cyan);
        _outputManager.WriteLine(room.Description, ConsoleColor.White);

        // Show exits
        var exits = new List<string>();
        if (room.NorthRoomId.HasValue) exits.Add("North");
        if (room.SouthRoomId.HasValue) exits.Add("South");
        if (room.EastRoomId.HasValue) exits.Add("East");
        if (room.WestRoomId.HasValue) exits.Add("West");

        if (exits.Any())
        {
            _outputManager.WriteLine($"Exits: {string.Join(", ", exits)}", ConsoleColor.Yellow);
        }
        else
        {
            _outputManager.WriteLine("No exits available.", ConsoleColor.Red);
        }

        // Show monsters
        if (room.Monsters.Any())
        {
            _outputManager.WriteLine($"Monsters here: {string.Join(", ", room.Monsters.Select(m => m.Name))}", ConsoleColor.Red);
        }

        // Show other players
        var otherPlayers = room.Players.Where(p => p.Id != player.Id).ToList();
        if (otherPlayers.Any())
        {
            _outputManager.WriteLine($"Players here: {string.Join(", ", otherPlayers.Select(p => p.Name))}", ConsoleColor.Green);
        }

        _outputManager.WriteLine("");
    }

}

