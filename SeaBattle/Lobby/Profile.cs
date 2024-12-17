using System.Text.Json.Serialization;

namespace SeaBattle.LobbyNamespace;

public record Profile
{
    public string Name { get; private set; }
    public int VictoryScore { get; private set; }
    public int DefeatScore { get; private set; }
    public int GamesPlayed { get; private set; }
    public bool IsGuest { get; private set; }

    [JsonConstructor]
    public Profile(string name, int victoryScore, int defeatScore, int gamesPlayed, bool isGuest)
    {
        Name = name;
        VictoryScore = victoryScore;
        DefeatScore = defeatScore;
        GamesPlayed = gamesPlayed;
        IsGuest = isGuest;
    }
    public Profile(string name, bool isGuest = false)
    {
        Name = name;
        VictoryScore = 0;
        DefeatScore = 0;
        GamesPlayed = 0;
        IsGuest = isGuest;
    }

    public void IncreaseVictoryScore()
    {
        VictoryScore++;
        GamesPlayed++;
    }
    public void IncreaseDefeatScore() 
    {
        DefeatScore++;
        GamesPlayed++;
    }

    public static Profile CreateGuestProfile()
    {
        return new Profile("Guest profile", true);
    }
}