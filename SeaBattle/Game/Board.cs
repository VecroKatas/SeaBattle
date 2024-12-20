namespace SeaBattle.GameNamespace;

public class Board
{
    public static int SideSize;
    public static int BiggestShipSize;

    private Tile[,] Tiles;
    private List<Ship> Ships;

    public List<Tile> TilesToCheckHit { get; private set; }
    public List<Tile> TilesToHit { get; private set; }

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

    public Board()
    {
        Tiles = new Tile[SideSize, SideSize];
        Ships = new List<Ship>();
        TilesToCheckHit = new List<Tile>();
        TilesToHit = new List<Tile>();
        
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
        
        if (tile.IsOccupied)
        {
            MarkNeighbouringTiles(coords);
            
            if (tile.Ship.CheckDestroyed())
            {
                DestroyAdjacentTiles(tile.Ship);
            }
            return true;
        }

        ShootTile(tile);
        
        return false;
    }

    private void MarkNeighbouringTiles(Vector2 coords)
    {
        int x = coords.X;
        int y = coords.Y;
        
        if (x > 0 && !Tiles[x - 1, y].IsShot)               AddTileToCheckMarked(x - 1, y);                         // l
        if (x < SideSize - 1 && !Tiles[x + 1, y].IsShot)    AddTileToCheckMarked(x + 1, y);                         // r
        if (y > 0 && !Tiles[x, y - 1].IsShot)               AddTileToCheckMarked(x, y - 1);                         // t
        if (y < SideSize - 1 && !Tiles[x, y + 1].IsShot)    AddTileToCheckMarked(x, y + 1);                         // b
    }

    private void AddTileToCheckMarked(int x, int y)
    {
        Tile markedTile = Tiles[x, y];
        if (!TilesToCheckHit.Contains(markedTile))
            TilesToCheckHit.Add(markedTile);
    }

    private void RemoveTileFromCheckMarked(Tile tile)
    {
        TilesToCheckHit.Remove(tile);
    }

    private void DestroyAdjacentTiles(Ship ship)
    {
        int x, y;
        foreach (var tile in ship.Tiles)
        {
            x = tile.X;
            y = tile.Y;
            
            if (x > 0)              ShootTile(Tiles[x - 1, y]);               // l
            if (x < SideSize - 1)   ShootTile(Tiles[x + 1, y]);               // r
            if (y > 0)              ShootTile(Tiles[x, y - 1]);               // t
            if (y < SideSize - 1)   ShootTile(Tiles[x, y + 1]);               // b

            if (x > 0 && y > 0)                         ShootTile(Tiles[x - 1, y - 1]);       // tl
            if (x < SideSize - 1 && y > 0)              ShootTile(Tiles[x + 1, y - 1]);       // tr
            if (x < SideSize - 1 && y < SideSize - 1)   ShootTile(Tiles[x + 1, y + 1]);       // br
            if (x > 0 && y < SideSize - 1)              ShootTile(Tiles[x - 1, y + 1]);       // bl
        }
    }

    private void ShootTile(Tile tile)
    {
        tile.Shoot();
        RemoveTileFromCheckMarked(tile);
        RemoveTileFromHitMarked(tile);
    }

    public Ship FindTraitor()
    {
        foreach (var ship in Ships)
        {
            if (ship is {IsTraitor: true, IsTraitorRevealed: false})
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

    public void GetScanned(Vector2 coords, int radarRadius)
    {
        for (int i = coords.X - radarRadius; i < coords.X + radarRadius; i++)
        {
            for (int j = coords.Y - radarRadius; j < coords.Y + radarRadius; j++)
            {
                if (AreCoordsWithinRadarRange(i, j, coords, radarRadius))
                {
                    Tile tile = Tiles[i, j];
                    if (tile is {IsOccupied: true, IsShot: false})
                        AddTileToHitMarked(tile);
                }
            }
        }
    }
    
    bool AreCoordsWithinRadarRange(int x, int y, Vector2 coords, int radarRadius)
    {
        int dx = Math.Abs(coords.X - x);
        int dy = Math.Abs(coords.Y - y);
        return dx * dx + dy * dy <= radarRadius * radarRadius;
    }

    private void AddTileToHitMarked(Tile tile)
    {
        if (!TilesToHit.Contains(tile))
            TilesToHit.Add(tile);
    }

    private void RemoveTileFromHitMarked(Tile tile)
    {
        TilesToHit.Remove(tile);
    }
}