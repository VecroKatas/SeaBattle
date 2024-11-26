namespace SeaBattle.Classes;

public static class InputHandler
{
    private static Random random = new Random();
    public static bool RequestManualBoardCreation()
    {
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
        Console.WriteLine("Is the ships orientation right or bottom? r/b");
        return Console.ReadKey().Key == ConsoleKey.R;
    }

    public static bool RequestRadarUsage()
    {
        Console.WriteLine("Do you want to use radar this turn instead of shooting? y/n");
        return Console.ReadKey().Key == ConsoleKey.Y;
    }

    public static Vector2 RequestShotCoords(bool isBot)
    {
        if (!isBot)
        {
            Console.WriteLine();
            Console.WriteLine("Choose a tile to shoot (e.g. a1)");
            return ReadCoords();
        }
        
        Vector2 randomCoords = new Vector2(random.Next(0, Board.SideSize), random.Next(0, Board.SideSize));

        randomCoords.StringRepresentation = TransformCoordsToString(randomCoords.X, randomCoords.Y);
        
        return randomCoords;
    }

    public static Vector2 RequestRadarCoords()
    {
        Console.WriteLine();
        Console.WriteLine("Choose a tile to be center of 4x4 radar scan (e.g. a1)");
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
            coords = TransformCoords(coordsInput);
        } while (!AreCoordsValid(coords));
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

    private static Vector2 TransformCoords(string coordsInput)
    {
        try
        {
            int y = coordsInput[0] - 'a';
            int x = Convert.ToInt32(coordsInput.Substring(1)) - 1;
            return new Vector2(x, y, coordsInput);
        }
        catch (Exception)
        {
            return new Vector2(-1, -1);
        }
    }

    private static string TransformCoordsToString(int x, int y)
    {
        string result = "";
        result += (char)(x + 'a');
        result += y;
        return result;
    }
}