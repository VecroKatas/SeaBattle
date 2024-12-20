using SeaBattle.LobbyNamespace;
using SeaBattle.Utilities;

namespace SeaBattle.GameNamespace;

public class Player
{
    public Board Board { get; set; }
    public bool IsHuman { get; private set; }
    public bool IsRadarAvailable { get; private set; } = true;

    public string Name { get; set; }
    
    public Player (){}

    public Player(bool isHuman, string name)
    {
        IsHuman = isHuman;
        Name = name;
    }

    public Player(Profile profile) : this(true, profile.Name) { }

    public virtual Vector2 GetShotCoords(Board opponentBoard)
    {
        return InputHandler.RequestShotCoords();
    }
    
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