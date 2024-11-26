namespace SeaBattle.Classes;

public class Game
{
    private ConsoleColor LastMissedShotColor = ConsoleColor.DarkGreen;
    private ConsoleColor LastHitColor = ConsoleColor.Red;
    private ConsoleColor ShotShipColor = ConsoleColor.DarkRed;
    private ConsoleColor ScannedColor = ConsoleColor.DarkYellow;
    private ConsoleColor TraitorShipColor = ConsoleColor.DarkBlue;
    
    private Random random = new Random();
    private Board board1; 
    private Board board2;
    private GameTurnInfo currentTurn;
    private int boardSize = 10;
    private int biggestShip = 4;
    private int radarRadius = 3;

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
            NotifyTurnResult();
        }

        DetermineWinner();
    }

    void Initialize()
    {
        currentTurn = new GameTurnInfo() {Board = board2, PreviousTurnHit = false, UseRadar = false, TraitorActed = false};
        
        Board.SideSize = boardSize;
        Board.BiggestShipSize = biggestShip;
        
        CreatePlayerBoard();

        CreateBotBoard();
    }

    bool GameRunning()
    {
        return board1.AreFightingShipsLeft && board2.AreFightingShipsLeft;
    }

    void ChangeTurn()
    {
        currentTurn.UseRadar = false;
        currentTurn.TraitorActed = false;
        
        currentTurn.Board = DetermineCurrentBoard(currentTurn.Board);
    }

    void Input()
    {
        bool useRadar = false;
        if (currentTurn.Board is {IsBotBoard: false, IsRadarAvailable: true})
            useRadar = InputHandler.RequestRadarUsage();

        Ship traitor = currentTurn.Board.FindTraitor();
        if (DoesTraitorAct(traitor))
        {
            currentTurn.UseRadar = true;
            currentTurn.TraitorActed = true;
            currentTurn.Coords = traitor.GetCoords();
            traitor.Betray();
            return;
        }

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

    bool DoesTraitorAct(Ship traitor)
    {
        return currentTurn.Board.IsBotBoard && random.Next(0, 100) < Ship.TreacheryChance && traitor != null && traitor.CanBetray();
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

    Vector2 GetShotCoords(Board board)
    {
        bool canShoot = false;
        Vector2 coords = new Vector2(-1, -1);
        while (!canShoot)
        {
            if (coords.X != -1 && !board.IsBotBoard)
                Console.WriteLine($"You cannot shoot {coords.StringRepresentation}.");
                
            coords = InputHandler.RequestShotCoords(board.IsBotBoard);

            canShoot = !GetOtherBoard(board).CheckIfTileIsShot(coords.X, coords.Y);
        }

        return coords;
    }

    Vector2 GetRadarCoords(Board board)
    {
        board.UseRadar();
        return InputHandler.RequestRadarCoords(radarRadius);
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
                    if (board.IsOpponentBoard && currentTurn.UseRadar && AreCoordsWithinRadarRange(j - 1, lineIndex - 1, radarRadius))
                    {
                        WriteScannedTile(board, j - 1, lineIndex - 1);
                    }
                    else
                    {
                        WriteTile(board, j - 1, lineIndex - 1);
                    }
                }
            }
        }
    }

    bool AreCoordsWithinRadarRange(int x, int y, int radius)
    {
        int dx = Math.Abs(currentTurn.Coords.X - x);
        int dy = Math.Abs(currentTurn.Coords.Y - y);
        return dx * dx + dy * dy <= radius * radius;
    }

    void WriteScannedTile(Board board, int x, int y)
    {
        char symbol = board.GetCurrentTileSymbol(x, y);
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
        char symbol = board.GetCurrentTileSymbol(x, y);
        bool isTraitorTile = IsTraitorShipTile(board, x, y);
        if (IsOpponentHiddenShipTile(board, symbol, isTraitorTile))
            symbol = Tile.RegularSymbol;

        if (IsPreviousShotTile(x, y, board))
            WritePreviousShotSymbol(symbol);
        else if (isTraitorTile)
            WriteTraitorSymbol(symbol);
        else
            WriteSymbol(symbol);
    }

    bool IsOpponentHiddenShipTile(Board board, char symbol, bool isTraitorTile)
    {
        return board.IsBotBoard && symbol == Tile.ShipSymbol && !isTraitorTile;
    }

    bool IsPreviousShotTile(int x, int y, Board board)
    {
        return (x, y) == (currentTurn.Coords.X, currentTurn.Coords.Y) && board != currentTurn.Board;
    }

    bool IsTraitorShipTile(Board board, int x, int y)
    {
        Ship traitor = board.GetShip(x, y);
        if (traitor != null)
            return traitor.IsTraitorRevealed;
        
        return false;
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
    
    void WriteTraitorSymbol(char symbol)
    {
        Console.ForegroundColor = TraitorShipColor;
        Console.Write(symbol);
        Console.ForegroundColor = ConsoleColor.White;
    }

    void NotifyTurnResult()
    {
        string result = currentTurn switch
        {
            {UseRadar: true, Board.IsOpponentBoard: false} =>
                $"You used radar centered on {currentTurn.Coords.StringRepresentation}.",
            {TraitorActed: true, Board.IsOpponentBoard: true} =>
                $"One of your opponents ships have betrayed them. It is located at {currentTurn.Coords.StringRepresentation}. You can win without destroying it.",
            {PreviousTurnHit: true, Board.IsOpponentBoard: false} =>
                $"Congratulations! You hit a ship at {currentTurn.Coords.StringRepresentation}. You get another turn!",
            {PreviousTurnHit: false, Board.IsOpponentBoard: false} =>
                $"You missed at {currentTurn.Coords.StringRepresentation}(",
            {PreviousTurnHit: true, Board.IsOpponentBoard: true} =>
                $"Womp womp! Enemy hit your ship at {currentTurn.Coords.StringRepresentation}. They get another turn!",
            {PreviousTurnHit: false, Board.IsOpponentBoard: true} =>
                $"Enemy missed at {currentTurn.Coords.StringRepresentation}!",
            _ => ""
        };
        Console.WriteLine(result);
    }

    void DetermineWinner()
    {
        if (!board1.AreFightingShipsLeft)
            Console.WriteLine("You lost boohoo!");
        else 
            Console.WriteLine("You won.");
    }

    struct GameTurnInfo
    {
        public Board Board;
        public bool PreviousTurnHit;
        public bool UseRadar;
        public bool TraitorActed;
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