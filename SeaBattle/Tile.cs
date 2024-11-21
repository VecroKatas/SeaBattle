namespace SeaBattle;

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
            if (!IsOccupied && IsShot) return ShotSymbol;
            if (IsOccupied && !IsShot) return ShipSymbol;
            if (IsOccupied && IsShot) return ShotShipSymbol;
            return RegularSymbol;
        }
    }
    public bool IsShot = false;
    public bool IsOccupied = false;
    public int X;
    public int Y;
    public Ship Ship;

    public Tile(int x, int y)
    {
        X = x;
        Y = y;
    }
}