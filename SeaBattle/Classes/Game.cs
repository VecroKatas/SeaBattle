namespace SeaBattle.Classes;

public struct Vector2
{
    public string StringRepresentation = "";
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

    public Vector2(string coordsInput)
    {
        SetCoords(coordsInput);
    }
    
    public void SetCoords(string coordsInput)
    {
        try
        {
            int y = coordsInput[0] - 'a';
            int x = Convert.ToInt32(coordsInput.Substring(1)) - 1;
            this = new Vector2(x, y, coordsInput);
        }
        catch (Exception)
        {
            this = new Vector2(-1, -1, "0-1");
        }
    }

    public string GetCoordsToString()
    {
        if (StringRepresentation == "")
        {
            string result = "";
            result += (char)(Y + 'a');
            result += X + 1;
            StringRepresentation = result;
        }
        return StringRepresentation;
    }
}
public enum GameMode
{
    PvP,
    PvE,
    EvE
}
public class Game
{
    private ConsoleColor LastMissedShotColor = ConsoleColor.DarkGreen;
    private ConsoleColor LastHitColor = ConsoleColor.Red;
    private ConsoleColor ShipColor = ConsoleColor.DarkBlue;
    private ConsoleColor ShotShipColor = ConsoleColor.DarkRed;
    private ConsoleColor ScannedColor = ConsoleColor.DarkYellow;
    private ConsoleColor TraitorShipColor = ConsoleColor.Magenta;
    
    private Random random = new Random();
    private GameMode _gamemode;
    
    private Player player1;
    private Player player2;
    
    private GameTurnInfo currentTurn;
    private GameTurnInfo previousTurn;

    private int roundCount;
    
    private int boardSize;
    private int biggestShip;
    private int radarRadius = 3;
    private int waitTime = 0;

    private bool firstPlayerWon;
    public Game(GameMode gamemode, Player player1, Player player2, bool bothGuests, int roundCount)
    {
        this.roundCount = roundCount;
        _gamemode = gamemode;
        
        this.player1 = player1;
        this.player2 = player2;

        if (bothGuests && player1.IsHuman && player2.IsHuman)
        {
            this.player1.Name += " 1";
            this.player2.Name += " 2";
        }
    }
    
    public void PlayGame()
    {
        Initialize();
        DisplayPreGameState();

        while (GameRunning())
        {
            Input();
            
            GameLogic();
            
            DisplayGameState();
        }

        DetermineWinner(); 
    }

    void Initialize()
    {
        Console.Clear();
        
        InitiateBoardsAndPlayers();
        
        currentTurn = new GameTurnInfo() 
            {CurrentPlayer = player1, OpponentPlayer = player2, TurnHit = false, UseRadar = false, TraitorActed = false};
        previousTurn = currentTurn;
    }

    void InitiateBoardsAndPlayers()
    {
        boardSize = 10;
        biggestShip = boardSize - 6;
        
        Board.SideSize = boardSize;
        Board.BiggestShipSize = biggestShip;
        
        (Board board1, Board board2) = _gamemode switch
        {
            GameMode.PvE => (CreatePlayerBoard(), CreateBotBoard()),
            GameMode.PvP => (CreatePlayerBoard(), CreatePlayerBoard()),
            GameMode.EvE => (CreateBotBoard(), CreateBotBoard()),
            _ => (CreatePlayerBoard(), CreateBotBoard())
        };

        player1.Board = board1;
        
        player2.Board = board2;
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
        InputHandler.NotifyPlayerTurn(currentTurn.CurrentPlayer.GetName());
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
    
    void DisplayPreGameState()
    {
        Console.WriteLine("Round №" + roundCount);
        ShowBoards(previousTurn);
    }
    
    void DisplayGameState()
    {
        ShowBoards(previousTurn);
        NotifyTurnResult();
        Wait();
        NotifyCurrentPlayer();
    }

    void ShowBoards(GameTurnInfo turnInfo)
    {
        Console.WriteLine();
        Console.WriteLine($" {player1.GetName()} board \t {player2.GetName()} board");
        
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
                Console.Write(j == 0 ? " " : j);
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
                Console.Write(j == 0 ? " " : j);
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
        return _gamemode switch
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
                $"One of {previousTurn.CurrentPlayer.GetName()}'s ships have betrayed them and refused to shoot. It is located at {previousTurn.Coords.GetCoordsToString()}." +
                $" The other player can win without destroying it.",
            {UseRadar: true} =>
                $"{previousTurn.CurrentPlayer.GetName()} used radar centered on {previousTurn.Coords.GetCoordsToString()}.",
            {TurnHit: true} =>
                $"{previousTurn.CurrentPlayer.GetName()} hit a ship at {previousTurn.Coords.GetCoordsToString()}. {previousTurn.CurrentPlayer.GetName()}" +
                $" gets another turn!",
            {TurnHit: false} =>
                $"{previousTurn.CurrentPlayer.GetName()} missed at {previousTurn.Coords.GetCoordsToString()}(",
        };
        Console.WriteLine(result);
    }

    void DetermineWinner()
    {
        if (DidPlayer1Win())
            Console.WriteLine($"{player1.GetName()} won!");
        else 
            Console.WriteLine($"{player2.GetName()} won!");
    }

    public bool DidPlayer1Win()
    {
        return player1.HasNotLost();
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