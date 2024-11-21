namespace SeaBattle;

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
                if (!Tiles[i].IsShot) return false;
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