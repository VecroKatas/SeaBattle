namespace SeaBattle.GameNamespace;

public class Tile
{
    public static char RegularSymbol => ' ';
    public static char ScannedSymbol => '.';
    public static char ShotSymbol => '*';
    public static char ShipSymbol => 'U';
    public static char ShotShipSymbol => '#';
    
    public bool IsShot { get; private set; } = false;
    
    public bool IsOccupied { get; private set; } = false;

    public bool AreNeighbouringTilesShot { get; private set; } = false;
    
    public char CurrentSymbol
    {
        get
        {
            if (!IsOccupied && IsShot) return ShotSymbol;
            if (IsOccupied && !IsShot) return ShipSymbol;
            if (IsOccupied && IsShot) return ShotShipSymbol;
            return RegularSymbol;
        }
    }

    public int X;
    public int Y;
    public Ship Ship;

    public Tile(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Shoot(){
        IsShot = true;
        if (IsOccupied)
            Ship.Hit();
    }

    public void Occupy() => IsOccupied = true;
}