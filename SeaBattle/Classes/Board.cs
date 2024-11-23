namespace SeaBattle.Classes;

public class Board
{
    public static int SideSize;
    public static int BiggestShipSize;

    public Tile[,] Tiles;
    public bool IsBotBoard;
    public List<Ship> Ships;

    public bool AreShipsNotDestroyed
    {
        get
        {
            foreach (var ship in Ships)
            {
                if (!ship.Destroyed) return true;
            }

            return false;
        }
    }

    public Board(bool isPlayerBoard)
    {
        Tiles = new Tile[SideSize, SideSize];
        IsBotBoard = !isPlayerBoard;
        Ships = new List<Ship>();
           
        GenerateEmptyBoard();
    }

    void GenerateEmptyBoard()
    {
        for (int i = 0; i < SideSize; i++)
        {
            for (int j = 0; j < SideSize; j++)
            {
                Tiles[i, j] = new Tile(j, i);
            }
        }
    }

    public bool CanCreateShip(Vector2 coords, int size, bool rightDirection)
    {
        int x = coords.X;
        int y = coords.Y;
        if (!IsTileAvailable(x, y)) return false;
        
        if (!rightDirection)
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
        
        if (!rightDirection)
        {
            for (int i = 0; i < size; i++)
            {
                ship.Tiles[i] = Tiles[coords.Y, coords.X + i];
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                ship.Tiles[i] = Tiles[coords.Y + i, coords.X];
            }
        }

        for (int i = 0; i < size; i++)
        {
            ship.Tiles[i].IsOccupied = true;
            ship.Tiles[i].Ship = ship;
        }

        Ships.Add(ship);
    }

    bool IsTileAvailable(int x, int y)
    {
        if (Tiles[y, x].IsOccupied) return false; //current available
            
        if (y > 0 && Tiles[y - 1, x].IsOccupied) return false;                                    // top available
        if (y < SideSize - 1 && Tiles[y + 1, x].IsOccupied) return false;                         // b
        if (x > 0 && Tiles[y, x - 1].IsOccupied) return false;                                    // l
        if (x < SideSize - 1 && Tiles[y, x + 1].IsOccupied) return false;                         // r

        if (y > 0 && x > 0 && Tiles[y - 1, x - 1].IsOccupied) return false;                       // tl
        if (y > 0 && x < SideSize - 1 && Tiles[y - 1, x + 1].IsOccupied) return false;            // tr
        if (y < SideSize - 1 && x < SideSize - 1 && Tiles[y + 1, x + 1].IsOccupied) return false; // br
        if (y < SideSize - 1 && x > 0 && Tiles[y + 1, x - 1].IsOccupied) return false;            // bl
            
        return true;
    }

    public char GetCurrentTileSymbol(int x, int y)
    {
        return Tiles[y, x].CurrentSymbol;
    }

    public bool CheckIfTileIsShot(int x, int y)
    {
        return Tiles[y, x].IsShot;
    }

    public bool Shoot(Vector2 coords)
    {
        Tile tile = Tiles[coords.Y, coords.X];
        
        tile.IsShot = true;
        
        if (tile.IsOccupied)
        {
            if (tile.Ship.Destroyed)
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
            
            if (y > 0) Tiles[y - 1, x].IsShot = true;                                    // t
            if (y < SideSize - 1) Tiles[y + 1, x].IsShot = true;                         // b
            if (x > 0) Tiles[y, x - 1].IsShot = true;                                    // l
            if (x < SideSize - 1) Tiles[y, x + 1].IsShot = true;                         // r

            if (y > 0 && x > 0) Tiles[y - 1, x - 1].IsShot = true;                       // tl
            if (y > 0 && x < SideSize - 1) Tiles[y - 1, x + 1].IsShot = true;            // tr
            if (y < SideSize - 1 && x < SideSize - 1) Tiles[y + 1, x + 1].IsShot = true; // br
            if (y < SideSize - 1 && x > 0) Tiles[y + 1, x - 1].IsShot = true;            // bl
        }
    }
}