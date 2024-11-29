using SeaBattle.Structs;

namespace SeaBattle.Classes;

public class Player
{
    public Board Board;
    public bool IsHuman { get; private set; }
    public bool IsRadarAvailable { get; private set; } = true;

    private string name;
    public string Name
    {
        get => name + (IsHuman ? " (human)" : " (bot)");
        set => name = value; 
    }

    public Player(bool isHuman, Board board, string name)
    {
        IsHuman = isHuman;
        Board = board;
        Name = name;
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
}