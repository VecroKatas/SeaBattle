using SeaBattle.GameNamespace;

namespace SeaBattle.Utilities;

public static class InputHandler
{
    private static Random random = new Random();

    public static ConsoleKeyInfo RequestGameModeKey()
    {
        Console.WriteLine("Choose game mode. Press 1/2/3 keys for PvE/PvP/EvE respectfully");
        ConsoleKeyInfo key = Console.ReadKey();
        Console.WriteLine();
        return key;
    }

    public static int RequestProfileIndex()
    {
        Console.WriteLine("Enter index of the profile you wish to play as");
        string indexStr;
        int index;
        bool parsed;
        do
        {
            indexStr = Console.ReadLine();
            parsed = int.TryParse(indexStr, out index);
            if (!parsed)
                Console.WriteLine("Wrong input. Try again parsing numbers");
        } while (!parsed);

        Console.WriteLine();
        return index;
    }

    public static ConsoleKeyInfo RequestBotDifficulty()
    {
        Console.WriteLine();
        Console.WriteLine("Choose bot difficulty. Press 1/2/3 keys for 'Random', 'Shooting suspected ship places' or 'Shooting suspected and scanned ship places' respectfully");
        ConsoleKeyInfo key = Console.ReadKey();
        Console.WriteLine();
        return key;
    }
    
    public static void NotifyPlayerTurn(string currentPlayerName)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(currentPlayerName + "'s turn");
    }
    
    public static bool RequestManualBoardCreation()
    {
        Console.WriteLine();
        Console.WriteLine("Do you want to manually create a board? (y/n)");
        ConsoleKey key = Console.ReadKey().Key;
        bool result = key == ConsoleKey.Y;
        Console.WriteLine();
        return result;
    }
    
    public static (Vector2 coords, bool isRightDirection) RequestInfoForShipCreation(int currentSize, bool isRepeatedRequest)
    {
        if (isRepeatedRequest) Console.WriteLine("Could not create the ship. Lets try again");

        Vector2 coords = RequestPlayerShipCoords(currentSize);
        bool isRightDirection = RequestIsRightDirection();
        
        Console.WriteLine();
        
        return (coords, isRightDirection);
    }
    
    private static Vector2 RequestPlayerShipCoords(int currentSize)
    {
        Console.WriteLine($"Size of the board is {Board.SideSize}x{Board.SideSize}; current ship size is {currentSize}");
        Console.WriteLine("Please, enter top left coordinate of the ship (e.g. a3)");
        return ReadCoords();
    }

    private static bool RequestIsRightDirection()
    {
        Console.WriteLine("Is the ships orientation right or bottom? (r/b)");
        return Console.ReadKey().Key == ConsoleKey.R;
    }

    public static bool RequestRadarUsage()
    {
        Console.WriteLine("Do you want to use radar this turn instead of shooting? (y/n)");
        return Console.ReadKey().Key == ConsoleKey.Y;
    }

    public static (bool useRadar, Vector2 coords) RequestTurnInput()
    {
        Console.WriteLine();
        Console.WriteLine("Write coords to shoot (e.g. a1), or to scan (e.g. radar a1)");
        string[] input = Console.ReadLine().Split(' ');

        if (input[0] == "radar")
        {
            return (true, ReadCoords(input[1]));
        }

        return (false, ReadCoords(input[0]));
    }

    public static Vector2 RequestShotCoords()
    {
        Console.WriteLine();
        Console.WriteLine("Choose a tile to shoot (e.g. a1)");
        return ReadCoords();
    }

    private static Vector2 ReadCoords()
    {
        string coordsInput;
        Vector2 coords = new Vector2(int.MinValue, Int32.MinValue);
        do
        {
            if (coords.X != int.MinValue)
                Console.WriteLine("Entered coords are not valid. Try again");
            coordsInput = Console.ReadLine();
            coords.SetCoords(coordsInput);
        } while (!AreCoordsValid(coords));
        return coords;
    }
    
    private static Vector2 ReadCoords(string coordsInput)
    {
        Vector2 coords = new Vector2(coordsInput);
        while (!AreCoordsValid(coords))
        {
            if (coords.X != int.MinValue)
                Console.WriteLine("Entered coords are not valid. Try again");
            coordsInput = Console.ReadLine();
            coords.SetCoords(coordsInput);
        }
        return coords;
    }

    private static bool AreCoordsValid(Vector2 coords)
    {
        return IsCoordValid(coords.X) && IsCoordValid(coords.Y);
    }

    private static bool IsCoordValid(int x)
    {
        return x > -1 && x < Board.SideSize;
    }

    public static void WaitForInput()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue");
        Console.ReadKey();
    }
}