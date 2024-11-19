class Tile
{
    public static char RegularSymbol => ' ';
    public static char ShotSymbol => '*';
    public static char ShipSymbol => 'U';
    public static char ShotShipSymbol => '#';

    public char CurrentSymbol
    {
        get
        {
            if (!Occupied && Shot) return ShotSymbol;
            if (Occupied && !Shot) return ShipSymbol;
            if (Occupied && Shot) return ShotShipSymbol;
            return RegularSymbol;
        }
    }
    public bool Shot = false;
    public bool Occupied = false;
    public int X;
    public int Y;
    public Ship Ship;

    public Tile(int x, int y)
    {
        X = x;
        Y = y;
    }
}
class Ship
{
    public Tile[] Tiles;
    public int Size;

    public bool Destroyed
    {
        get
        {
            for (int i = 0; i < Size; i++)
            {
                if (!Tiles[i].Shot) return false;
            }

            return true;
        }
    }

    public Ship(int size)
    {
        Size = size;
        Tiles = new Tile[size];
    }
}

class Board
{
    public Tile[,] Tiles;
    public int SideSize;
    public int BiggestShipSize;
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

    public bool IsEnemyBoard;

    public Board(int size, int biggestShipSize, bool isEnemyBoard)
    {
        Tiles = new Tile[size, size];
        SideSize = size;
        BiggestShipSize = biggestShipSize;
        IsEnemyBoard = isEnemyBoard;
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

    public Ship CreateShip(int x, int y, int size, bool rightDirection)
    {
        Ship ship = new Ship(size);
        
        if (!IsTileAvailable(x, y)) return null;
        
        if (rightDirection)
        {
            if (x + size > SideSize) return null;
                
            for (int i = 0; i < size; i++)
            {
                if (!IsTileAvailable(x + i, y)) return null;
                ship.Tiles[i] = Tiles[y, x + i];
            }
        }
        else
        {
            if (y + size > SideSize) return null;
            
            for (int i = 0; i < size; i++)
            {
                if (!IsTileAvailable(x, y + i)) return null;
                ship.Tiles[i] = Tiles[y + i, x];
            }
        }

        for (int i = 0; i < size; i++)
        {
            ship.Tiles[i].Occupied = true;
            ship.Tiles[i].Ship = ship;
        }

        Ships.Add(ship);
        return ship;
    }

    bool IsTileAvailable(int x, int y)
    {
        if (Tiles[y, x].Occupied) return false; //current available
            
        if (y > 0 && Tiles[y - 1, x].Occupied) return false;                                    // top available
        if (y < SideSize - 1 && Tiles[y + 1, x].Occupied) return false;                         // b
        if (x > 0 && Tiles[y, x - 1].Occupied) return false;                                    // l
        if (x < SideSize - 1 && Tiles[y, x + 1].Occupied) return false;                         // r

        if (y > 0 && x > 0 && Tiles[y - 1, x - 1].Occupied) return false;                       // tl
        if (y > 0 && x < SideSize - 1 && Tiles[y - 1, x + 1].Occupied) return false;            // tr
        if (y < SideSize - 1 && x < SideSize - 1 && Tiles[y + 1, x + 1].Occupied) return false; // br
        if (y < SideSize - 1 && x > 0 && Tiles[y + 1, x - 1].Occupied) return false;            // bl
            
        return true;
    }

    public bool TryHit(int x, int y)
    {
        Tile tile = Tiles[y, x];
        
        tile.Shot = true;
        
        if (tile.Occupied)
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
            
            if (y > 0) Tiles[y - 1, x].Shot = true;                                    // t
            if (y < SideSize - 1) Tiles[y + 1, x].Shot = true;                         // b
            if (x > 0) Tiles[y, x - 1].Shot = true;                                    // l
            if (x < SideSize - 1) Tiles[y, x + 1].Shot = true;                         // r

            if (y > 0 && x > 0) Tiles[y - 1, x - 1].Shot = true;                       // tl
            if (y > 0 && x < SideSize - 1) Tiles[y - 1, x + 1].Shot = true;            // tr
            if (y < SideSize - 1 && x < SideSize - 1) Tiles[y + 1, x + 1].Shot = true; // br
            if (y < SideSize - 1 && x > 0) Tiles[y + 1, x - 1].Shot = true;            // bl
        }
    }
}