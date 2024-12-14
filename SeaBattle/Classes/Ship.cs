namespace SeaBattle.Classes;

public class Ship
{
    public static int TraitorSpawnChance = 5;
    public static int TreacheryChance = 20;
    
    public Tile[] Tiles;
    public int Size;

    public bool IsDestroyed { get; private set; }
    public bool IsTraitor { get; private set; }
    public bool IsTraitorRevealed { get; private set; }
    public bool IsHit { get; private set; } = false;

    private Random random = new Random();

    public Ship(int size)
    {
        Size = size;
        Tiles = new Tile[size];
        IsTraitor = random.Next(0, 100) < TraitorSpawnChance;
    }

    public bool CheckDestroyed()
    {
        for (int i = 0; i < Size; i++)
        {
            if (!Tiles[i].IsShot)
            {
                IsDestroyed = false;
                return IsDestroyed;
            }
        }

        IsDestroyed = true;
        return IsDestroyed;
    }

    public void Hit()
    {
        IsHit = true;
    }

    public bool CanBetray()
    {
        return !IsHit && IsTraitor;
    }

    public void Betray()
    {
        IsTraitorRevealed = true;
    }

    public Vector2 GetCoords()
    {
        return new Vector2(Tiles[0].X, Tiles[0].Y);
    }
}