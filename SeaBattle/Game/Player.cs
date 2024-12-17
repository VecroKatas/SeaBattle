using SeaBattle.LobbyNamespace;

namespace SeaBattle.GameNamespace;

public class Player
{
    public Board Board { get; set; }
    public bool IsHuman { get; private set; }
    public bool IsRadarAvailable { get; private set; } = true;

    public string Name { get; set; }

    public Player(bool isHuman, Board board, string name)
    {
        IsHuman = isHuman;
        Board = board;
        Name = name;
    }

    public Player(bool isHuman, string name)
    {
        IsHuman = isHuman;
        Name = name;
    }

    public Player(Profile profile) : this(true, profile.Name) { }

    public bool HasNotLost()
    {
        return Board.AreFightingShipsLeft;
    }

    public void UseRadar()
    {
        IsRadarAvailable = false;
    }

    public bool GetShot(Vector2 coords)
    {
        return Board.Shoot(coords);
    }

    public string GetName()
    {
        return Name + (IsHuman ? " (human)" : " (bot)");
    }
}