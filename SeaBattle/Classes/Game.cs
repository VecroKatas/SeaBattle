using SeaBattle.Structs;

namespace SeaBattle.Classes;

public class Game
{
    private ConsoleColor LastMissedShotColor = ConsoleColor.DarkGreen;
    private ConsoleColor LastHitColor = ConsoleColor.Red;
    private ConsoleColor ShipColor = ConsoleColor.DarkBlue;
    private ConsoleColor ShotShipColor = ConsoleColor.DarkRed;
    private ConsoleColor ScannedColor = ConsoleColor.DarkYellow;
    private ConsoleColor TraitorShipColor = ConsoleColor.Magenta;
    
    private Random random = new Random();
    private GameMode _gameMode;
    private Player player1;
    private Player player2;
    private GameTurnInfo currentTurn;
    private GameTurnInfo previousTurn;
    private int boardSize;
    private int biggestShip;
    private int radarRadius = 3;
    private int waitTime = 800;
    private bool firstTurn = true;

    private enum GameMode
    {
        PvP,
        PvE,
        EvE
    }
    
    public void StartGame()
    {
        Initialize();
        DisplayGameState();

        while (GameRunning())
        {
            if (_gameMode == GameMode.PvP)
                DisplayPvPGameState();
            NotifyCurrentPlayer();
            
            Input();
            
            GameCycle();
            
            DisplayGameState();
            NotifyTurnResult();
            Wait();
        }

        DetermineWinner(); 
    }

    void Initialize()
    {
        SetGameMode();

        InitiateBoards();
        
        currentTurn = new GameTurnInfo() 
            {CurrentPlayer = player1, OpponentPlayer = player2, TurnHit = false, UseRadar = false, TraitorActed = false};
        previousTurn = currentTurn;
    }

    void SetGameMode()
    {
        ConsoleKeyInfo key = InputHandler.RequestGameModeKey();
        _gameMode = key.Key switch
        {
            ConsoleKey.D1 => GameMode.PvE,
            ConsoleKey.D2 => GameMode.PvP,
            ConsoleKey.D3 => GameMode.EvE,
            _ => GameMode.PvE
        };
    }

    void InitiateBoards()
    {
        boardSize = 10;
        biggestShip = boardSize - 6;
        
        Board.SideSize = boardSize;
        Board.BiggestShipSize = biggestShip;
        
        (Board board1, Board board2, bool isHumanBoard1, bool isHumanBoard2) = _gameMode switch
        {
            GameMode.PvE => (CreatePlayerBoard(), CreateBotBoard(), true, false),
            GameMode.PvP => (CreatePlayerBoard(), CreatePlayerBoard(), true, true),
            GameMode.EvE => (CreateBotBoard(), CreateBotBoard(), false, false),
            _ => (CreatePlayerBoard(), CreateBotBoard(), true, false)
        };
        
        player1 = new Player(isHumanBoard1, board1, "Player 1");
        player2 = new Player(isHumanBoard2, board2, "Player 2");
        
        board1.SetPlayer(player1);
        board2.SetPlayer(player2);
    }
    
    bool GameRunning()
    {
        return player1.HasNotLost() && player2.HasNotLost();
    }

    void Wait()
    {
        if (_gameMode == GameMode.PvP && CanPassTurn())
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to pass the turn");
            Console.ReadKey();
        }
        else
        {
            System.Threading.Thread.Sleep(waitTime);
        }
    }

    bool CanPassTurn()
    {
        return !previousTurn.TurnHit || previousTurn.UseRadar;
    }

    void NotifyCurrentPlayer()
    {
        InputHandler.NotifyPlayerTurn(currentTurn.CurrentPlayer.Name);
    }

    void Input()
    {
        bool useRadar = false;
        if (currentTurn.CurrentPlayer is {IsHuman: true, IsRadarAvailable: true})
            useRadar = InputHandler.RequestRadarUsage();

        if (useRadar)
        {
            currentTurn.UseRadar = true;
            currentTurn.Coords = GetRadarCoords(currentTurn.CurrentPlayer);
        }
        else
        {
            currentTurn.Coords = GetShotCoords(currentTurn.CurrentPlayer);
        }
    }

    void GameCycle()
    {
        Ship traitor = currentTurn.CurrentPlayer.Board.FindTraitor();
        if (DoesTraitorAct(traitor))
        {
            currentTurn.UseRadar = true;
            currentTurn.TraitorActed = true;
            currentTurn.Coords = traitor.GetCoords();
            traitor.Betray();
        }
        
        if (!currentTurn.UseRadar)
        {
            bool isHit = currentTurn.OpponentPlayer.GetShot(currentTurn.Coords);
        
            currentTurn.TurnHit = isHit;
        }
        
        ChangeTurn();
    }

    bool DoesTraitorAct(Ship traitor)
    {
        return random.Next(0, 100) < Ship.TreacheryChance && traitor != null && traitor.CanBetray();
    }

    void ChangeTurn()
    {
        previousTurn = currentTurn;
        
        currentTurn.UseRadar = false;
        currentTurn.TraitorActed = false;
        currentTurn.TurnHit = false;
        
        currentTurn.CurrentPlayer = DetermineCurrentPlayer();
        currentTurn.OpponentPlayer = GetOtherPlayer(currentTurn.CurrentPlayer);
    }

    Board CreatePlayerBoard()
    {
        Board board = GenerateBoard(InputHandler.RequestManualBoardCreation());
        
        Console.Clear();

        return board;
    }

    Board CreateBotBoard()
    {
        return GenerateBoard( false);
    }

    Board GenerateBoard(bool doManualGeneration)
    {
        Board newBoard = new Board();

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
                    ShowCreationBoard(board);
            }
        }
        
        if (doManualGeneration)
            Console.Clear();
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

    Player DetermineCurrentPlayer()
    {
        if (previousTurn.TurnHit) 
            return previousTurn.CurrentPlayer;

        return previousTurn.OpponentPlayer;
    }

    Player GetOtherPlayer(Player player)
    {
        return player == player1 ? player2 : player1;
    }

    Vector2 GetRadarCoords(Player player)
    {
        player.UseRadar();
        return InputHandler.RequestRadarCoords(radarRadius);
    }

    Vector2 GetShotCoords(Player player)
    {
        bool canShoot = false;
        Vector2 coords = new Vector2(-1, -1);
        while (!canShoot)
        {
            if (coords.X != -1 && player.IsHuman)
                Console.WriteLine($"You cannot shoot {coords.StringRepresentation}.");
                
            coords = InputHandler.RequestShotCoords(player.IsHuman);

            canShoot = !GetOtherPlayer(player).Board.CheckIfTileIsShot(coords.X, coords.Y);
        }

        return coords;
    }

    void DisplayPvPGameState()
    {
        Console.Clear();
        if (firstTurn)
            firstTurn = false;
        else
            NotifyTurnResult();
        
        ShowBoards(currentTurn);
    }
    
    void DisplayGameState()
    {
        ShowBoards(previousTurn);
    }

    void ShowBoards(GameTurnInfo turnInfo)
    {
        Console.WriteLine();
        Console.WriteLine($" {player1.Name} board \t {player2.Name} board");
        for (int i = 0; i < Board.SideSize + 1; i++)
        {   
            ShowBoardLine(player1.Board, i, turnInfo);
            Console.Write("\t\t");
            ShowBoardLine(player2.Board, i, turnInfo);
            Console.WriteLine();
        }
    }
    
    void ShowCreationBoard(Board board)
    {
        Console.WriteLine();
        for (int i = 0; i < Board.SideSize + 1; i++)
        {   
            ShowCreationBoardLine(board, i, currentTurn);
            Console.WriteLine();
        }
    }
    
    void ShowCreationBoardLine(Board board, int lineIndex, GameTurnInfo turnInfo)
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
                    WriteCreationTile(board, j - 1, lineIndex - 1);
                }
            }
        }
    }

    void ShowBoardLine(Board board, int lineIndex, GameTurnInfo turnInfo)
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
                    if (IsTileWithinTraitorRadar(board, j - 1, lineIndex - 1, turnInfo))
                    {
                        WriteScannedTile(board, j - 1, lineIndex - 1);
                    }
                    else if (IsOpponentTileWithinRadar(board, j - 1, lineIndex - 1, turnInfo))
                    {
                        WriteScannedTile(board, j - 1, lineIndex - 1);
                    }
                    else
                    {
                        WriteTile(board, j - 1, lineIndex - 1, turnInfo);
                    }
                }
            }
        }
    }

    bool IsTileWithinTraitorRadar(Board board, int columnIndex, int lineIndex, GameTurnInfo turnInfo)
    {
        return turnInfo.TraitorActed && !IsOpponentBoard(board, turnInfo) && turnInfo.UseRadar &&
               AreCoordsWithinRadarRange(columnIndex - 1, lineIndex - 1, turnInfo);
    }

    bool IsOpponentTileWithinRadar(Board board, int columnIndex, int lineIndex, GameTurnInfo turnInfo)
    {
        return !turnInfo.TraitorActed && IsOpponentBoard(board, turnInfo) && turnInfo.UseRadar &&
               AreCoordsWithinRadarRange(columnIndex - 1, lineIndex - 1, turnInfo);
    }

    bool IsOpponentBoard(Board board, GameTurnInfo turnInfo)
    {
        if (board.Player == turnInfo.CurrentPlayer)
            return false;

        return true;
    }

    bool AreCoordsWithinRadarRange(int x, int y, GameTurnInfo turnInfo)
    {
        int dx = Math.Abs(turnInfo.Coords.X - x);
        int dy = Math.Abs(turnInfo.Coords.Y - y);
        return dx * dx + dy * dy <= radarRadius * radarRadius;
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
    
    void WriteCreationTile(Board board, int x, int y)
    {
        char symbol = board.GetCurrentTileSymbol(x, y);

        WriteSymbol(symbol);
    }

    void WriteTile(Board board, int x, int y, GameTurnInfo turnInfo)
    {
        char symbol = board.GetCurrentTileSymbol(x, y);
        bool isTraitorTile = IsTraitorShipTile(board, x, y);
        if (HideShipTile(board, symbol, isTraitorTile, turnInfo))
            symbol = Tile.RegularSymbol;

        if (IsJustShotTile(x, y, board, turnInfo))
            WriteJustShotSymbol(symbol);
        else if (isTraitorTile)
            WriteTraitorSymbol(symbol);
        else
            WriteSymbol(symbol);
    }

    bool HideShipTile(Board board, char symbol, bool isTraitorTile, GameTurnInfo turnInfo)
    {
        return _gameMode switch
        {
            GameMode.PvE => IsBotBoard(board) && symbol == Tile.ShipSymbol && !isTraitorTile,
            GameMode.PvP => IsOpponentBoard(board, turnInfo) && symbol == Tile.ShipSymbol && !isTraitorTile,
            GameMode.EvE => false
        };
    }

    bool IsBotBoard(Board board)
    {
        return !board.Player.IsHuman;
    }

    bool IsJustShotTile(int x, int y, Board board, GameTurnInfo turnInfo)
    {
        return (x, y) == (turnInfo.Coords.X, turnInfo.Coords.Y) && board != turnInfo.CurrentPlayer.Board;
    }

    bool IsTraitorShipTile(Board board, int x, int y)
    {
        Ship traitor = board.GetShip(x, y);
        if (traitor != null)
            return traitor.IsTraitorRevealed;
        
        return false;
    }

    void WriteJustShotSymbol(char symbol)
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
        else if (symbol == Tile.ShipSymbol)
            Console.ForegroundColor = ShipColor;
        
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
        string result = previousTurn switch
        {
            {TraitorActed: true} =>
                $"One of {previousTurn.CurrentPlayer.Name}'s ships have betrayed them. It is located at {previousTurn.Coords.StringRepresentation}." +
                $" You can win without destroying it.",
            {UseRadar: true} =>
                $"{previousTurn.CurrentPlayer.Name} used radar centered on {previousTurn.Coords.StringRepresentation}.",
            {TurnHit: true} =>
                $"{previousTurn.CurrentPlayer.Name} hit a ship at {previousTurn.Coords.StringRepresentation}. {previousTurn.CurrentPlayer.Name}" +
                $" gets another turn!",
            {TurnHit: false} =>
                $"{previousTurn.CurrentPlayer.Name} missed at {previousTurn.Coords.StringRepresentation}(",
        };
        Console.WriteLine(result);
    }

    void DetermineWinner()
    {
        if (!player1.HasNotLost())
            Console.WriteLine($"{player1.Name} won!");
        else 
            Console.WriteLine($"{player2.Name} won!");
    }

    struct GameTurnInfo
    {
        public Player CurrentPlayer;
        public Player OpponentPlayer;
        public bool TurnHit;
        public bool UseRadar;
        public bool TraitorActed;
        public Vector2 Coords;
    }
}