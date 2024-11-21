namespace SeaBattle;

public class Game
{
    private Random random = new Random();
    private Board board1;
    private Board board2;
    private GameTurnInfo previousTurn;
    private string lastCoords;

    public Game()
    {
        previousTurn = new GameTurnInfo() {Board = board2, PreviousTurnHit = false};
    }
    
    public void StartGame()
    {
        CreateBoards();
        ShowBoards();

        GameCycle();

        DetermineWinner();
    }

    void CreateBoards()
    {
        CreatePlayerBoard();

        CreateBotBoard();
    }

    void CreatePlayerBoard()
    {
        board1 = GenerateBoard(10, 4, true, !RequestManualBoardCreation());
    }

    void CreateBotBoard()
    {
        board2 = GenerateBoard(10, 4, false, true);
    }

    bool RequestManualBoardCreation()
    {
        Console.WriteLine("Do you want to manually create a board? (y/n)");
        ConsoleKey key = Console.ReadKey().Key;
        return key == ConsoleKey.Y;
    }

    Board GenerateBoard(int size, int biggestShipSize, bool isPlayer, bool doRandomGenerate)
    {
        Board newBoard = new Board(size, biggestShipSize, isPlayer);

        GenerateShips(newBoard, doRandomGenerate);
        
        return newBoard;
    }

     void GenerateShips(Board board, bool doRandomGenerate)
    {
        for (int i = Board.BiggestShipSize; i > 0; i--)
        {
            for (int j = 0; j < Board.BiggestShipSize - i + 1; j++)
            {
                Ship ship;
                bool isRepeatedRequest = false;
                do
                {
                    (int x, int y, bool isRightDirection) = GetShipCharacteristics(i, doRandomGenerate, isRepeatedRequest); 
                    ship = board.CreateShip(x, y, i, isRightDirection);
                    isRepeatedRequest = true;
                } while (ship == null);
                
                if (!doRandomGenerate)
                    ShowBoard(board);
            }
        }
    }

     (int x, int y, bool isRightDirection) GetShipCharacteristics(int shipSize, bool doRandomGenerate, bool isRepeatedRequest)
    {
        if (!doRandomGenerate)
        {
            (int x, int y, bool isRightDirection) = RequestInputForShipCreation(shipSize, isRepeatedRequest);
            return (x - 1, y - 1, isRightDirection);
        }
        return (random.Next(0, Board.SideSize - shipSize + 1), random.Next(0, Board.SideSize - shipSize + 1), random.Next(0, 2) == 1);
    }

     (int x, int y, bool isRightDirection) RequestInputForShipCreation(int shipSize, bool repeated)
    {
        if (repeated) Console.WriteLine("Could not create the ship. Lets try again");
        
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
        string coords = Console.ReadLine();
        lastCoords = coords;
        return TransformCoords(coords);
    }

     (int x, int y) TransformCoords(string coords)
    {
        int y = coords[0] - 'a';
        int x = Convert.ToInt32(coords.Substring(1)) - 1;
        return (x, y);
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

     void ShowBoards()
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
                    Console.Write(Convert.ToChar(96 + lineIndex));
                }
                else
                {
                    char symbol = board.Tiles[lineIndex - 1, j - 1].CurrentSymbol;
                    if (board.IsBotBoard && symbol == Tile.ShipSymbol) symbol = Tile.RegularSymbol;  
                    Console.Write(symbol);
                }
            }
        }
    }

     void GameCycle()
     {
         while (NeitherBoardLost())
         {
             GameTurn();
             ShowBoards();
             NotifyTurnResult(previousTurn);
         }
     }

     bool NeitherBoardLost()
    {
        return board1.AreShipsNotDestroyed && board2.AreShipsNotDestroyed;
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
        
        previousTurn = currentTurn;
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
    }
}