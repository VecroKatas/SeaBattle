namespace SeaBattle.Classes;

public class Game
{
    private ConsoleColor LastMissedShotColor = ConsoleColor.DarkGreen;
    private ConsoleColor LastHitColor = ConsoleColor.Red;
    private ConsoleColor ShotShipColor = ConsoleColor.DarkRed;
    private ConsoleColor ScannedColor = ConsoleColor.DarkYellow;
    
    private Random random = new Random();
    private Board board1; 
    private Board board2;
    private GameTurnInfo currentTurn;
    private int radarRange = 3;

    public Game() { }
    
    public void StartGame()
    {
        Initialize();
        DisplayGameState();

        while (GameRunning())
        {
            ChangeTurn();
            Input();
            GameCycle();
            DisplayGameState();
        }

        DetermineWinner();
    }

    void Initialize()
    {
        currentTurn = new GameTurnInfo() {Board = board2, PreviousTurnHit = false, UseRadar = false};
        
        Board.SideSize = 10;
        Board.BiggestShipSize = 4;
        
        CreatePlayerBoard();

        CreateBotBoard();
    }

    bool GameRunning()
    {
        return board1.AreShipsNotDestroyed && board2.AreShipsNotDestroyed;
    }

    void ChangeTurn()
    {
        currentTurn.UseRadar = false;
        
        currentTurn.Board = DetermineCurrentBoard(currentTurn.Board);
    }

    void Input()
    {
        bool useRadar = false;
        if (currentTurn.Board is {IsBotBoard: false, IsRadarAvailable: true})
            useRadar = InputHandler.RequestRadarUsage();

        if (useRadar)
        {
            currentTurn.UseRadar = true;
            currentTurn.Coords = GetRadarCoords(currentTurn.Board);
        }
        else
        {
            currentTurn.Coords = GetShotCoords(currentTurn.Board);
        }
    }

    void GameCycle()
    {
        if (!currentTurn.UseRadar)
        {
            bool isHit = GetOtherBoard(currentTurn.Board).Shoot(currentTurn.Coords);
        
            currentTurn.PreviousTurnHit = isHit;
        }
    }

    void CreatePlayerBoard()
    {
        board1 = GenerateBoard(true, false, InputHandler.RequestManualBoardCreation());
    }

    void CreateBotBoard()
    {
        board2 = GenerateBoard(false, true, false);
    }

    Board GenerateBoard(bool isPlayer, bool isOpponent, bool doManualGeneration)
    {
        Board newBoard = new Board(isPlayer, isOpponent);

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

    Vector2 GetRadarCoords(Board board)
    {
        board.UseRadar();
        return InputHandler.RequestRadarCoords();
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
                    if (board.IsOpponentBoard && currentTurn.UseRadar && AreCoordsWithinRadarRange(j, lineIndex))
                        WriteScannedTile(board, j, lineIndex);
                    else
                        WriteTile(board, j, lineIndex);
                }
            }
        }
    }

    bool AreCoordsWithinRadarRange(int x, int y)
    {
        int dx = Math.Abs(currentTurn.Coords.X - x);
        int dy = Math.Abs(currentTurn.Coords.Y - y);
        return dx < radarRange && dy < radarRange;
    }

    void WriteScannedTile(Board board, int x, int y)
    {
        char symbol = board.GetCurrentTileSymbol(x - 1, y - 1);
        if (symbol == Tile.RegularSymbol) 
            symbol = Tile.ScannedSymbol;

        WriteScannedSymbol(symbol);
    }
    
    void WriteScannedSymbol(char symbol)
    {
        Console.ForegroundColor = ScannedColor;
        Console.Write(symbol);
        Console.ForegroundColor = ConsoleColor.White;
    }

    void WriteTile(Board board, int x, int y)
    {
        char symbol = board.GetCurrentTileSymbol(x - 1, y - 1);
        if (board.IsBotBoard && symbol == Tile.ShipSymbol) 
            symbol = Tile.RegularSymbol;

        if ((x - 1, y - 1) == (currentTurn.Coords.X, currentTurn.Coords.Y))
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

    void NotifyTurnResult()
    {
        if (currentTurn is {PreviousTurnHit: true, Board.IsOpponentBoard: false})
            Console.WriteLine($"Congratulations! You hit a ship at {currentTurn.Coords.StringRepresentation}. You get another turn!");
        else if (currentTurn is {PreviousTurnHit: false, Board.IsOpponentBoard: false})
            Console.WriteLine($"You missed at {currentTurn.Coords.StringRepresentation}(");
        else if (currentTurn is {PreviousTurnHit: true, Board.IsOpponentBoard: true})
            Console.WriteLine($"Womp womp! Enemy hit your ship at {currentTurn.Coords.StringRepresentation}. They get another turn!");
        else if (currentTurn is {PreviousTurnHit: false, Board.IsOpponentBoard: true})
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
        public bool UseRadar;
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