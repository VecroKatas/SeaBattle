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
    private int waitTime = 1000;
    private bool firstTurn = true;

    private enum GameMode
    {
        PvP,
        PvE,
        EvE
    }
    
    public Game(){}

    public Game(Player player1, Player player2)
    {
        this.player1 = player1;
        this.player2 = player2;
    }
    
    public void StartGame()
    {
        Initialize();
        DisplayGameState();

        while (GameRunning())
        {
            NotifyCurrentPlayer();
            
            Input();
            
            GameLogic();
            
            DisplayGameState();
            NotifyTurnResult();
            Wait();
        }

        DetermineWinner(); 
    }

    void Initialize()
    {
        SetGameMode();

        InitiateBoardsAndPlayers();
        
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

    void InitiateBoardsAndPlayers()
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

        if (player1 == null)
            player1 = new Player(isHumanBoard1, board1, "Player 1");
        
        if (player2 == null)
            player2 = new Player(isHumanBoard2, board2, "Player 2");
        
        board1.SetPlayer(player1);
        board2.SetPlayer(player2);
    }
    /*
        винести ініціалізацію гравців у GameManager
        зробити створення нового гравця якщо нічого не задано, або присвоєння борди до заданого гравця
        автоматичне визначення режиму гри, базуючись на введенних гравцях
        гейм менеджер має зберігати дані про кількість перемог кожного гравця, граємо best of 3
    
    */
    
    
    void SetBoardToPlayer(Player player, Board board)
    {
        player.Board = board;
        
        board.SetPlayer(player);
    }
    
    bool GameRunning()
    {
        return player1.HasNotLost() && player2.HasNotLost();
    }

    void Wait()
    {
        System.Threading.Thread.Sleep(waitTime);
    }

    void NotifyCurrentPlayer()
    {
        InputHandler.NotifyPlayerTurn(currentTurn.CurrentPlayer.Name);
    }

    void Input()
    {
        currentTurn.Coords = GetTurnCoords(currentTurn.CurrentPlayer);
    }

    void GameLogic()
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
        
        ShowCreationBoard(board);
        
        Wait();
        
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

    Vector2 GetTurnCoords(Player player)
    {
        if (player is {IsRadarAvailable: true, IsHuman: true})
        {
            (bool useRadar, Vector2 coords) = InputHandler.RequestTurnInput();
            if (useRadar)
            {
                currentTurn.UseRadar = true;
                player.UseRadar();
                return coords;
            }
            return GetShotCoords(player, coords);
        }
        
        return GetShotCoords(player, new Vector2(-1, -1));
    }

    Vector2 GetShotCoords(Player player, Vector2 coords)
    {
        bool canShoot = false;
        if (coords.X != -1)
            canShoot = !GetOtherPlayer(player).Board.CheckIfTileIsShot(coords.X, coords.Y);
        
        while (!canShoot)
        {
            if (coords.X != -1 && player.IsHuman)
                Console.WriteLine($"You cannot shoot {coords.StringRepresentation}.");
            
            if (player.IsHuman)
                coords = InputHandler.RequestShotCoords();
            else
                coords = new Vector2(random.Next(0, Board.SideSize), random.Next(0, Board.SideSize));

            canShoot = !GetOtherPlayer(player).Board.CheckIfTileIsShot(coords.X, coords.Y);
        }

        return coords;
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
            ShowBoardLine(player1.Board, IsOpponentPlayer(player1, turnInfo), !player1.IsHuman, i, turnInfo);
            Console.Write("\t\t");
            ShowBoardLine(player2.Board, IsOpponentPlayer(player2, turnInfo), !player2.IsHuman, i, turnInfo);
            Console.WriteLine();
        }
    }
    
    void ShowCreationBoard(Board board)
    {
        Console.WriteLine();
        for (int i = 0; i < Board.SideSize + 1; i++)
        {   
            ShowCreationBoardLine(board, i);
            Console.WriteLine();
        }
    }
    
    void ShowCreationBoardLine(Board board, int lineIndex)
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

    void ShowBoardLine(Board board, bool isOpponentBoard, bool isBotPlayer, int lineIndex, GameTurnInfo turnInfo)
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
                    if (IsTileWithinTraitorRadar(isOpponentBoard, j - 1, lineIndex - 1, turnInfo))
                    {
                        WriteScannedTile(board, j - 1, lineIndex - 1);
                    }
                    else if (IsOpponentTileWithinRadar(isOpponentBoard, j - 1, lineIndex - 1, turnInfo))
                    {
                        WriteScannedTile(board, j - 1, lineIndex - 1);
                    }
                    else
                    {
                        WriteTile(board, isBotPlayer, j - 1, lineIndex - 1, turnInfo);
                    }
                }
            }
        }
    }

    bool IsTileWithinTraitorRadar(bool isOpponentBoard, int columnIndex, int lineIndex, GameTurnInfo turnInfo)
    {
        return turnInfo.TraitorActed && !isOpponentBoard && turnInfo.UseRadar &&
               AreCoordsWithinRadarRange(columnIndex, lineIndex, turnInfo);
    }

    bool IsOpponentTileWithinRadar(bool isOpponentBoard, int columnIndex, int lineIndex, GameTurnInfo turnInfo)
    {
        return !turnInfo.TraitorActed && isOpponentBoard && turnInfo.UseRadar &&
               AreCoordsWithinRadarRange(columnIndex, lineIndex, turnInfo);
    }

    bool IsOpponentPlayer(Player player, GameTurnInfo turnInfo)
    {
        if (player == turnInfo.CurrentPlayer)
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

    void WriteTile(Board board, bool isBotPlayer, int x, int y, GameTurnInfo turnInfo)
    {
        char symbol = board.GetCurrentTileSymbol(x, y);
        bool isTraitorTile = IsTraitorShipTile(board, x, y);
        if (ShouldHideShipTile(isBotPlayer, symbol, isTraitorTile))
            symbol = Tile.RegularSymbol;

        if (IsJustShotTile(x, y, board, turnInfo))
            WriteJustShotSymbol(symbol);
        else if (isTraitorTile)
            WriteTraitorSymbol(symbol);
        else
            WriteSymbol(symbol);
    }

    bool ShouldHideShipTile(bool isBotPlayer, char symbol, bool isTraitorTile)
    {
        return _gameMode switch
        {
            GameMode.PvE => isBotPlayer && symbol == Tile.ShipSymbol && !isTraitorTile,
            GameMode.PvP => symbol == Tile.ShipSymbol && !isTraitorTile,
            GameMode.EvE => false
        };
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
                $"One of {previousTurn.CurrentPlayer.Name}'s ships have betrayed them and refused to shoot. It is located at {previousTurn.Coords.GetCoordsToString()}." +
                $" The other player can win without destroying it.",
            {UseRadar: true} =>
                $"{previousTurn.CurrentPlayer.Name} used radar centered on {previousTurn.Coords.GetCoordsToString()}.",
            {TurnHit: true} =>
                $"{previousTurn.CurrentPlayer.Name} hit a ship at {previousTurn.Coords.GetCoordsToString()}. {previousTurn.CurrentPlayer.Name}" +
                $" gets another turn!",
            {TurnHit: false} =>
                $"{previousTurn.CurrentPlayer.Name} missed at {previousTurn.Coords.GetCoordsToString()}(",
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