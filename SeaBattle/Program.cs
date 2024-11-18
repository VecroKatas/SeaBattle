namespace SeaBattle;

class Program
{
    class Tile
    {
        public bool Shot;
        public Ship? Ship;
    }

    class Board
    {
        public Tile[,] Map;
        public int SideSize;

        public Board(int size)
        {
            Map = new Tile[size, size];
            SideSize = size;
        }

        Ship CreateShip(int x, int y, int size, bool rightDirection)
        {
            Ship ship = new Ship(size);
            if (rightDirection)
            {
                if (x + size > SideSize) return null;
                
                for (int i = 0; i < size; i++)
                {
                    ship.Tiles[i] = Map[y, x + i];
                    Map[y, x + i].Ship = ship;
                }
            }
            else
            {
                if (y + size > SideSize) return null;
                
                for (int i = 0; i < size; i++)
                {
                    ship.Tiles[i] = Map[y + i, x];
                    Map[y + i, x].Ship = ship;
                }
            }

            return ship;
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
    
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}