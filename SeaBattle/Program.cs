namespace SeaBattle;

class Program
{
    private static Random random = new Random();

    static Board GenerateBoard(int size, int biggestShipSize, bool isPlayer, bool doRandomGenerate)
    {
        Board newBoard = new Board(size, biggestShipSize, !isPlayer);

        GenerateShips(newBoard, doRandomGenerate);
        
        return newBoard;
    }

    static void GenerateShips(Board board, bool doRandomGenerate)
    {
        for (int i = board.BiggestShipSize; i > 0; i--)
        {
            for (int j = 0; j < board.BiggestShipSize - i + 1; j++)
            {
                Ship ship;
                bool repeated = false;
                do
                {
                    (int x, int y, bool isRightDirection) = GetShipCharacteristics(i, board.SideSize, doRandomGenerate, repeated); 
                    ship = board.CreateShip(x, y, i, isRightDirection);
                    repeated = true;
                } while (ship == null);
                
                if (!doRandomGenerate)
                    ShowBoard(board);
            }
        }
    }

    static (int x, int y, bool isRightDirection) GetShipCharacteristics(int shipSize, int sideSize, bool doRandomGenerate, bool repeated)
    {
        if (!doRandomGenerate)
        {
            (int x, int y, bool isRightDirection) = RequestInputForShipCreation(shipSize, sideSize, repeated);
            return (x - 1, y - 1, isRightDirection);
        }
        return (random.Next(0, sideSize - shipSize + 1), random.Next(0, sideSize - shipSize + 1), random.Next(0, 2) == 1);
    }

    static (int x, int y, bool isRightDirection) RequestInputForShipCreation(int shipSize, int sideSize, bool repeated)
    {
        if (repeated) Console.WriteLine("Could not create the ship. Lets try again");
        
        Console.WriteLine($"Size of the board is {sideSize}x{sideSize}; current ship size is {shipSize}");
        Console.WriteLine("Please, enter top left coordinate of the ship (e.g. a3)");
        int x, y;
        (x, y) = RequestCoords();
        
        Console.WriteLine("Is the ships orientation right or bottom? r/b");
        bool isRightDirection = Console.ReadKey().Key == ConsoleKey.R;
        Console.WriteLine();
        return (x, y, isRightDirection);
    }

    static (int x, int y) RequestCoords()
    {
        string coords = Console.ReadLine();
        return TransformCoords(coords);
    }

    static (int x, int y) TransformCoords(string coords)
    {
        int y = coords[0] - 97;
        int x = Convert.ToInt32(coords.Substring(1)) - 1;
        return (x, y);
    }
    
    static void ShowBoard(Board board)
    {
        Console.WriteLine();
        for (int i = 0; i < board.SideSize + 1; i++)
        {   
            ShowBoardLine(board, i);
            Console.WriteLine();
        }
    }

    static void ShowBoards(Board playerBoard, Board enemyBoard)
    {
        Console.WriteLine();
        Console.WriteLine(" My board \t\t\t\t\t\t Enemy board");
        for (int i = 0; i < playerBoard.SideSize + 1; i++)
        {   
            ShowBoardLine(playerBoard, i);
            Console.Write("\t\t\t\t\t");
            ShowBoardLine(enemyBoard, i);
            Console.WriteLine();
        }
    }

    static void ShowBoardLine(Board board, int lineIndex)
    {
        for (int j = 0; j < board.SideSize + 1; j++)
        {
            Console.Write(' ');
            if (lineIndex == 0)
            {
                Console.Write(j);
            }
            else
            {
                if (j == 0)
                {
                    Console.Write(Convert.ToChar(96 + lineIndex));
                }
                else
                {
                    char symbol = board.Tiles[lineIndex - 1, j - 1].CurrentSymbol;
                    //if (board.IsEnemyBoard && symbol == Tile.ShipSymbol) symbol = Tile.RegularSymbol;  
                    Console.Write(symbol);
                }
            }
        }
    }

    static bool NeitherBoardLost(Board playerBoard, Board enemyBoard)
    {
        return playerBoard.AreShipsNotDestroyed && enemyBoard.AreShipsNotDestroyed;
    }

    static (int x, int y) RequestPlayerShotCoords()
    {
        Console.WriteLine("Choose a tile to shoot (e.g. a1)");
        return RequestCoords();
    }

    static GameTurnInfo GameTurn(Board playerBoard, Board enemyBoard, GameTurnInfo previousTurnInfo)
    {
        GameTurnInfo turnInfo;
        turnInfo.IsPlayerTurn = previousTurnInfo.PreviousTurnHit ? previousTurnInfo.IsPlayerTurn : !previousTurnInfo.IsPlayerTurn;

        bool hit = false;
        if (turnInfo.IsPlayerTurn)
        {
            int x, y;
            (x, y) = RequestPlayerShotCoords();

            if (enemyBoard.Tiles[y, x].Shot)
            {
                Console.WriteLine("You cannot shoot that what is already shot.");
                return previousTurnInfo;
            }

            hit = enemyBoard.TryHit(x, y);
        }
        else
        {
            int x = random.Next(0, playerBoard.SideSize);
            int y = random.Next(0, playerBoard.SideSize);
            
            hit = playerBoard.TryHit(x, y);
        }

        turnInfo.PreviousTurnHit = hit;
        
        return turnInfo;
    }

    static void StartGame()
    {
        Console.WriteLine("Do you want to manually create a board? (y/n)");
        ConsoleKey key = Console.ReadKey().Key;
        Console.WriteLine();
        Board playerBoard = key == ConsoleKey.Y ? GenerateBoard(10, 4, true, false) : GenerateBoard(10, 4, true, true);

        Board enemyBoard = GenerateBoard(10, 4, false, true);
        
        ShowBoards(playerBoard, enemyBoard);

        GameTurnInfo previousTurn = new GameTurnInfo() {PreviousTurnHit = false, IsPlayerTurn = false};
        while (NeitherBoardLost(playerBoard, enemyBoard))
        {
            previousTurn = GameTurn(playerBoard, enemyBoard, previousTurn);
            
            ShowBoards(playerBoard, enemyBoard);
            NotifyTurnResult(previousTurn);
        }

        DetermineWinner(playerBoard, enemyBoard);
    }

    static void NotifyTurnResult(GameTurnInfo turnInfo)
    {
        if (turnInfo is {PreviousTurnHit: true, IsPlayerTurn: true})
            Console.WriteLine("Congratulations! You hit a ship. You get another turn!");
        else if (turnInfo is {PreviousTurnHit: false, IsPlayerTurn: true})
            Console.WriteLine("You missed(");
        else if (turnInfo is {PreviousTurnHit: true, IsPlayerTurn: false})
            Console.WriteLine("Womp womp! Enemy hit your ship. They get another turn!");
        else if (turnInfo is {PreviousTurnHit: false, IsPlayerTurn: false})
            Console.WriteLine("Enemy missed!");
    }

    static void DetermineWinner(Board playerBoard, Board enemyBoard)
    {
        if (!playerBoard.AreShipsNotDestroyed)
            Console.WriteLine("You lost boohoo!");
        else 
            Console.WriteLine("You won.");
    }

    struct GameTurnInfo
    {
        public bool IsPlayerTurn;
        public bool PreviousTurnHit;
    }
    
    static void Main(string[] args)
    {
        StartGame();
    }
}