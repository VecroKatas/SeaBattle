namespace SeaBattle.Classes;

public class Ship
{
    public Tile[] Tiles;
    public int Size;

    public bool IsDestroyed { get; private set; }

    public Ship(int size)
    {
        Size = size;
        Tiles = new Tile[size];
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
}