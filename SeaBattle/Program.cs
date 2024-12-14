using SeaBattle.Classes;

namespace SeaBattle;

class Program
{
    static void Main(string[] args)
    {
        Lobby lobby = new Lobby();
        lobby.StartNewGame();
    }
}