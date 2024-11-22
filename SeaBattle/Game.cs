namespace SeaBattle;

public class Game
{
    private Random random = new Random();
    private Board board1; 
    private Board board2;
    private GameTurnInfo previousTurn;
    private string lastCoords;
    private ConsoleColor LastMissedShotColor = ConsoleColor.DarkGreen;
    private ConsoleColor LastHitColor = ConsoleColor.Red;
    private ConsoleColor ShotShipColor = ConsoleColor.DarkRed;

    public Game()
    {
        previousTurn = new GameTurnInfo() {Board = board2, PreviousTurnHit = false};
    }
    
    public void StartGame()
    {
        Initialize();
        DisplayGameState();

        while (NeitherLost())
        {
            GameTurn();
            DisplayGameState();
            NotifyTurnResult(previousTurn);
        }

        DetermineWinner();
    }

    void Initialize()
    {
        Board.SideSize = 10;
        Board.BiggestShipSize = 4;
        
        CreatePlayerBoard();

        CreateBotBoard();
    }

    void GameTurn()
    {
        GameTurnInfo currentTurn;
        currentTurn.Board = DetermineCurrentBoard(previousTurn.Board);

        bool hit;
        int x, y;
        (x, y) = GetShotCoords(currentTurn.Board);
        hit = GetOtherBoard(currentTurn.Board).TryHit(x, y);
        
        currentTurn.PreviousTurnHit = hit;

        (currentTurn.ShootX, currentTurn.ShootY) = (x, y);
        
        previousTurn = currentTurn;
    }

    void CreatePlayerBoard()
    {
        board1 = GenerateBoard(true, RequestManualBoardCreation());
    }

    void CreateBotBoard()
    {
        board2 = GenerateBoard(false, true);
    }

    bool RequestManualBoardCreation()
    {
        Console.WriteLine("Do you want to manually create a board? (y/n)");
        ConsoleKey key = Console.ReadKey().Key;
        return key == ConsoleKey.Y;
    }

    Board GenerateBoard(bool isPlayer, bool doManualGeneration)
    {
        Board newBoard = new Board(isPlayer);

        GenerateShips(newBoard, doManualGeneration);
        
        return newBoard;
    }

    void GenerateShips(Board board, bool doManualGeneration)
    {
        for (int i = Board.BiggestShipSize; i > 0; i--)
        {
            for (int j = 0; j < Board.BiggestShipSize - i + 1; j++)
            {
                Ship ship;
                bool isRepeatedRequest = false;
                do
                {
                    (int x, int y, bool isRightDirection) = GetShipCharacteristics(i, doManualGeneration, isRepeatedRequest); 
                    ship = board.CreateShip(x, y, i, isRightDirection);
                    isRepeatedRequest = true;
                } while (ship == null);
                
                if (doManualGeneration)
                    ShowBoard(board);
            }
        }
    }

    (int x, int y, bool isRightDirection) GetShipCharacteristics(int shipSize, bool doManualGeneration, bool isRepeatedRequest)
    {
        if (doManualGeneration)
        {
            (int x, int y, bool isRightDirection) = RequestInfoForShipCreation(shipSize, isRepeatedRequest);
            return (x - 1, y - 1, isRightDirection);
        }
        return (random.Next(0, Board.SideSize - shipSize + 1), random.Next(0, Board.SideSize - shipSize + 1), random.Next(0, 2) == 1);
    }

    (int x, int y, bool isRightDirection) RequestInfoForShipCreation(int shipSize, bool isRepeatedRequest)
    {
        if (isRepeatedRequest) Console.WriteLine("Could not create the ship. Lets try again");
        
        Console.WriteLine($"Size of the board is {Board.SideSize}x{Board.SideSize}; current ship size is {shipSize}");
        Console.WriteLine("Please, enter top left coordinate of the ship (e.g. a3)");
        int x, y;
        (x, y) = ReadCoords();
        
        Console.WriteLine("Is the ships orientation right or bottom? r/b");
        bool isRightDirection = Console.ReadKey().Key == ConsoleKey.R;
        Console.WriteLine();
        return (x, y, isRightDirection);
    }

    (int x, int y) ReadCoords()
    {
        string coords;
        int x = int.MinValue;
        int y;
        do
        {
            if (x != int.MinValue)
                Console.WriteLine("Entered coords are not valid. Try again");
            coords = Console.ReadLine();
            (x, y) = TransformCoords(coords);
        } while (!AreCoordsValid(x, y));
        lastCoords = coords;
        return (x, y);
    }

    bool AreCoordsValid(int x, int y)
    {
        return x > -1 && x < Board.SideSize && y > -1 && y < Board.SideSize;
    }

    (int x, int y) TransformCoords(string coords)
    {
        try
        {
            int y = coords[0] - 'a';
            int x = Convert.ToInt32(coords.Substring(1)) - 1;
            return (x, y);
        }
        catch (Exception e)
        {
            return (-1, -1);
        }
    }
    
    void ShowBoard(Board board)
    {
        Console.WriteLine();
        for (int i = 0; i < Board.SideSize + 1; i++)
        {   
            ShowBoardLine(board, i);
            Console.WriteLine();
        }
    }

    void ShowBoardLine(Board board, int lineIndex)
    {
        for (int j = 0; j < Board.SideSize + 1; j++)
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
                    Console.Write(Convert.ToChar('a' + lineIndex - 1));
                }
                else
                {
                    WriteTileSymbol(board, lineIndex, j);
                }
            }
        }
    }

    void WriteTileSymbol(Board board, int lineIndex, int colIndex)
    {
        char symbol = board.GetCurrentTileSymbol(lineIndex - 1, colIndex - 1);
        if (board.IsBotBoard && symbol == Tile.ShipSymbol) 
            symbol = Tile.RegularSymbol;

        if ((lineIndex - 1, colIndex - 1) == (previousTurn.ShootY, previousTurn.ShootX))
            WritePreviousShotSymbol(symbol);
        else
            WriteSymbol(symbol);
    }

    void WritePreviousShotSymbol(char symbol)
    {
        if (symbol == Tile.ShotSymbol) 
            Console.ForegroundColor = LastMissedShotColor;
        if (symbol == Tile.ShotShipSymbol) 
            Console.ForegroundColor = LastHitColor;
        Console.Write(symbol);
        Console.ForegroundColor = ConsoleColor.White;
    }

    void WriteSymbol(char symbol)
    {
        if (symbol == Tile.ShotShipSymbol)
            Console.ForegroundColor = ShotShipColor;
        Console.Write(symbol);
        Console.ForegroundColor = ConsoleColor.White;
    }

    void DisplayGameState()
    {
        Console.WriteLine();
        Console.WriteLine(" My board \t\t\t\t\t\t Enemy board");
        for (int i = 0; i < Board.SideSize + 1; i++)
        {   
            ShowBoardLine(board1, i);
            Console.Write("\t\t\t\t\t");
            ShowBoardLine(board2, i);
            Console.WriteLine();
        }
    }

    bool NeitherLost()
    {
        return board1.AreShipsNotDestroyed && board2.AreShipsNotDestroyed;
    }

    Board DetermineCurrentBoard(Board previousBoard)
    {
        if (previousTurn.PreviousTurnHit) return previousBoard;
        return GetOtherBoard(previousBoard);
    }

    Board GetOtherBoard(Board board)
    {
        return board == board1 ? board2 : board1;
    }

    (int x, int y) GetShotCoords(Board currentBoard)
    {
        bool canShoot = false;
        int x = -1;
        int y = -1;
        while (!canShoot)
        {
            if (!currentBoard.IsBotBoard)
            {
                if (x != -1)
                    Console.WriteLine($"You cannot shoot {lastCoords}.");
                
                (x, y) = RequestPlayerShotCoords();
            }
            else
            {
                (x, y) = (random.Next(0, Board.SideSize), random.Next(0, Board.SideSize));
                lastCoords = Convert.ToChar(y + 'a') + "" + (x + 1);
            }

            canShoot = !GetOtherBoard(currentBoard).CheckIfTileIsShot(x, y);
        }

        return (x, y);
    }

    (int x, int y) RequestPlayerShotCoords()
    {
        Console.WriteLine("Choose a tile to shoot (e.g. a1)");
        return ReadCoords();
    }

    void NotifyTurnResult(GameTurnInfo turnInfo)
    {
        if (turnInfo is {PreviousTurnHit: true, Board.IsBotBoard: false})
            Console.WriteLine($"Congratulations! You hit a ship at {lastCoords}. You get another turn!");
        else if (turnInfo is {PreviousTurnHit: false, Board.IsBotBoard: false})
            Console.WriteLine($"You missed at {lastCoords}(");
        else if (turnInfo is {PreviousTurnHit: true, Board.IsBotBoard: true})
            Console.WriteLine($"Womp womp! Enemy hit your ship at {lastCoords}. They get another turn!");
        else if (turnInfo is {PreviousTurnHit: false, Board.IsBotBoard: true})
            Console.WriteLine($"Enemy missed at {lastCoords}!");
    }

    void DetermineWinner()
    {
        if (!board1.AreShipsNotDestroyed)
            Console.WriteLine("You lost boohoo!");
        else 
            Console.WriteLine("You won.");
    }

    struct GameTurnInfo
    {
        public Board Board;
        public bool PreviousTurnHit;
        public int ShootX;
        public int ShootY;
    }
}