namespace SeaBattle.GameNamespace;

public enum BotDifficulty
{
    Random,
    ShootingNearbyHit,
    ShootingNearbyHitAndRadar,
}

public class BotPlayer : Player
{
    private Random random = new Random();
    private BotDifficulty _difficulty;

    public BotPlayer(BotDifficulty botDifficulty, bool isHuman, string name) : base (isHuman, name)
    {
        _difficulty = botDifficulty;
    }
    
    public override Vector2 GetShotCoords(Board opponentBoard)
    {
        return _difficulty switch
        {
            BotDifficulty.Random => GetRandomCoords(),
            BotDifficulty.ShootingNearbyHit => GetNearbyHitCoords(opponentBoard),
            BotDifficulty.ShootingNearbyHitAndRadar => GetNearbyHitAndRadarCoords(opponentBoard),
            _ => GetRandomCoords()
        };
    }

    private Vector2 GetRandomCoords()
    {
        return new Vector2(random.Next(0, Board.SideSize), random.Next(0, Board.SideSize));
    }

    private Vector2 GetNearbyHitCoords(Board opponentBoard)
    {
        List<Tile> markedTiles = opponentBoard.TilesToCheckHit;

        if (markedTiles.Count > 0)
        {
            Tile tile = markedTiles[random.Next(markedTiles.Count)];
            return new Vector2(tile.X, tile.Y);
        }

        return GetRandomCoords();
    }

    private Vector2 GetNearbyHitAndRadarCoords(Board opponentBoard)
    {
        List<Tile> markedTiles = opponentBoard.TilesToHit;

        if (markedTiles.Count > 0)
        {
            Tile tile = markedTiles[random.Next(markedTiles.Count)];
            return new Vector2(tile.X, tile.Y);
        }

        return GetNearbyHitCoords(opponentBoard);
    }
}