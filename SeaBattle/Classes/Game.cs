namespace SeaBattle.Classes;

public class Game
{
    private Random random = new Random();
    private Board board1; 
    private Board board2;
    private GameTurnInfo currentTurn;
    private ConsoleColor LastMissedShotColor = ConsoleColor.DarkGreen;
    private ConsoleColor LastHitColor = ConsoleColor.Red;
    private ConsoleColor ShotShipColor = ConsoleColor.DarkRed;

    public Game()
    {
        currentTurn = new GameTurnInfo() {Board = board2, PreviousTurnHit = false};
    }
    
    public void StartGame()
    {
        Initialize();
        DisplayGameState();

        while (GameRunning())
        {
            Input();
            GameCycle();
            DisplayGameState();
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

    void Input()
    {
        currentTurn.Board = DetermineCurrentBoard(currentTurn.Board);

        currentTurn.Coords = GetShotCoords(currentTurn.Board);
    }

    void GameCycle()
    {
        bool isHit = GetOtherBoard(currentTurn.Board).Shoot(currentTurn.Coords);
        
        currentTurn.PreviousTurnHit = isHit;
    }

    void CreatePlayerBoard()
    {
        board1 = GenerateBoard(true, InputHandler.RequestManualBoardCreation());
    }

    void CreateBotBoard()
    {
        board2 = GenerateBoard(false, false);
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
                bool isRepeatedRequest = false;
                bool isRightDirection;
                Vector2 coords;
                do
                {
                    (coords, isRightDirection) = GetShipCharacteristics(i, doManualGeneration, isRepeatedRequest);
                    isRepeatedRequest = true;
                } while (!board.CanCreateShip(coords, i, isRightDirection));
                board.CreateShip(coords, i, isRightDirection);
                
                if (doManualGeneration)
                    ShowBoard(board);
            }
        }
    }

    (Vector2 coords, bool isRightDirection) GetShipCharacteristics(int shipSize, bool doManualGeneration, bool isRepeatedRequest)
    {
        if (doManualGeneration)
        {
            (Vector2 coords, bool isRightDirection) = InputHandler.RequestInfoForShipCreation(shipSize, isRepeatedRequest);
            return (coords, isRightDirection);
        }

        Vector2 randomCoords = new Vector2(random.Next(0, Board.SideSize - shipSize + 1), random.Next(0, Board.SideSize - shipSize + 1));
        return (randomCoords, random.Next(0, 2) == 1);
    }

    bool GameRunning()
    {
        return board1.AreShipsNotDestroyed && board2.AreShipsNotDestroyed;
    }

    Board DetermineCurrentBoard(Board previousBoard)
    {
        if (currentTurn.PreviousTurnHit) return previousBoard;
        return GetOtherBoard(previousBoard);
    }

    Board GetOtherBoard(Board board)
    {
        return board == board1 ? board2 : board1;
    }

    Vector2 GetShotCoords(Board currentBoard)
    {
        bool canShoot = false;
        Vector2 coords = new Vector2(-1, -1);
        while (!canShoot)
        {
            if (coords.X != -1)
                Console.WriteLine($"You cannot shoot {coords.StringRepresentation}.");
                
            coords = InputHandler.RequestShotCoords(currentBoard.IsBotBoard);

            canShoot = !GetOtherBoard(currentBoard).CheckIfTileIsShot(coords.X, coords.Y);
        }

        return coords;
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

        if ((lineIndex - 1, colIndex - 1) == (currentTurn.Coords.X, currentTurn.Coords.Y))
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

        NotifyTurnResult();
    }

    void NotifyTurnResult()
    {
        if (currentTurn is {PreviousTurnHit: true, Board.IsBotBoard: false})
            Console.WriteLine($"Congratulations! You hit a ship at {currentTurn.Coords.StringRepresentation}. You get another turn!");
        else if (currentTurn is {PreviousTurnHit: false, Board.IsBotBoard: false})
            Console.WriteLine($"You missed at {currentTurn.Coords.StringRepresentation}(");
        else if (currentTurn is {PreviousTurnHit: true, Board.IsBotBoard: true})
            Console.WriteLine($"Womp womp! Enemy hit your ship at {currentTurn.Coords.StringRepresentation}. They get another turn!");
        else if (currentTurn is {PreviousTurnHit: false, Board.IsBotBoard: true})
            Console.WriteLine($"Enemy missed at {currentTurn.Coords.StringRepresentation}!");
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
        public Vector2 Coords;
    }
}

public struct Vector2
{
    public string StringRepresentation;
    public int X;
    public int Y;

    public Vector2(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public Vector2(int x, int y, string coordsInput) : this(x, y)
    {
        StringRepresentation = coordsInput;
    }
}