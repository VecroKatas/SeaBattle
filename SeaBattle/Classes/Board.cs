namespace SeaBattle.Classes;

public class Board
{
    public static int SideSize;
    public static int BiggestShipSize;

    public Tile[,] Tiles;
    public bool IsBotBoard { get; private set; }
    public bool IsOpponentBoard { get; private set; }
    public bool IsRadarAvailable { get; private set; } = true;
    public List<Ship> Ships;

    public bool AreFightingShipsLeft
    {
        get
        {
            foreach (var ship in Ships)
            {
                if (!ship.IsDestroyed && !ship.IsTraitorRevealed) return true;
            }

            return false;
        }
    }

    public Board(bool isPlayerBoard, bool isOpponentBoard)
    {
        Tiles = new Tile[SideSize, SideSize];
        IsBotBoard = !isPlayerBoard;
        IsOpponentBoard = isOpponentBoard;
        Ships = new List<Ship>();
           
        GenerateEmptyBoard();
    }

    void GenerateEmptyBoard()
    {
        for (int i = 0; i < SideSize; i++)
        {
            for (int j = 0; j < SideSize; j++)
            {
                Tiles[i, j] = new Tile(i, j);
            }
        }
    }

    public bool CanCreateShip(Vector2 coords, int size, bool rightDirection)
    {
        int x = coords.X;
        int y = coords.Y;
        if (!IsTileAvailable(x, y)) return false;
        
        if (rightDirection)
        {
            if (x + size > SideSize) return false;
                
            for (int i = 0; i < size; i++)
            {
                if (!IsTileAvailable(x + i, y)) return false;
            }
        }
        else
        {
            if (y + size > SideSize) return false;
            
            for (int i = 0; i < size; i++)
            {
                if (!IsTileAvailable(x, y + i)) return false;
            }
        }
        
        return true;
    }

    public void CreateShip(Vector2 coords, int size, bool rightDirection)
    {
        Ship ship = new Ship(size);
        
        if (rightDirection)
        {
            for (int i = 0; i < size; i++)
            {
                ship.Tiles[i] = Tiles[coords.X + i, coords.Y];
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                ship.Tiles[i] = Tiles[coords.X, coords.Y + i];
            }
        }

        for (int i = 0; i < size; i++)
        {
            ship.Tiles[i].Occupy();
            ship.Tiles[i].Ship = ship;
        }

        Ships.Add(ship);
    }

    bool IsTileAvailable(int x, int y)
    {
        if (Tiles[x, y].IsOccupied) return false; //current available
            
        if (x > 0 && Tiles[x - 1, y].IsOccupied) return false;                                    // l
        if (x < SideSize - 1 && Tiles[x + 1, y].IsOccupied) return false;                         // r
        if (y > 0 && Tiles[x, y - 1].IsOccupied) return false;                                    // t
        if (y < SideSize - 1 && Tiles[x, y + 1].IsOccupied) return false;                         // b

        if (x > 0 && y > 0 && Tiles[x - 1, y - 1].IsOccupied) return false;                       // tl
        if (x < SideSize - 1 && y > 0 && Tiles[x + 1, y - 1].IsOccupied) return false;            // tr
        if (x < SideSize - 1 && y < SideSize - 1 && Tiles[x + 1, y + 1].IsOccupied) return false; // br
        if (x > 0 && y < SideSize - 1 && Tiles[x - 1, y + 1].IsOccupied) return false;            // bl
            
        return true;
    }

    public char GetCurrentTileSymbol(int x, int y)
    {
        return Tiles[x, y].CurrentSymbol;
    }

    public bool CheckIfTileIsShot(int x, int y)
    {
        return Tiles[x, y].IsShot;
    }

    public bool Shoot(Vector2 coords)
    {
        Tile tile = Tiles[coords.X, coords.Y];

        tile.Shoot();
        
        if (tile.IsOccupied)
        {
            if (tile.Ship.CheckDestroyed())
            {
                DestroyAdjacentTiles(tile.Ship);
            }
            return true;
        }
        
        return false;
    }

    private void DestroyAdjacentTiles(Ship ship)
    {
        int x, y;
        foreach (var tile in ship.Tiles)
        {
            x = tile.X;
            y = tile.Y;
            
            if (x > 0) Tiles[x - 1, y].Shoot();                                    // l
            if (x < SideSize - 1) Tiles[x + 1, y].Shoot();                         // r
            if (y > 0) Tiles[x, y - 1].Shoot();                                    // t
            if (y < SideSize - 1) Tiles[x, y + 1].Shoot();                         // b

            if (x > 0 && y > 0) Tiles[x - 1, y - 1].Shoot();                       // tl
            if (x < SideSize - 1 && y > 0) Tiles[x + 1, y - 1].Shoot();            // tr
            if (x < SideSize - 1 && y < SideSize - 1) Tiles[x + 1, y + 1].Shoot(); // br
            if (x > 0 && y < SideSize - 1) Tiles[x - 1, y + 1].Shoot();            // bl
        }
    }

    public void UseRadar()
    {
        IsRadarAvailable = false;
    }

    public Ship FindTraitor()
    {
        foreach (var ship in Ships)
        {
            if (ship.IsTraitor && !ship.IsTraitorRevealed)
            {
                return ship;
            }
        }

        return null;
    }

    public Ship GetShip(int x, int y)
    {
        return Tiles[x, y].Ship;
    }
}