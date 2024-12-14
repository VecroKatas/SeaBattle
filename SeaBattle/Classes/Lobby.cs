namespace SeaBattle.Classes;

public class Lobby
{
    private Profile _profile1;
    private Profile _profile2;

    private Player _player1;
    private Player _player2;

    private int player1Wins = 0;
    private int player2Wins = 0;
    private int maxWins = 3;

    private GameMode _gameMode;
    
    public Lobby(Profile profile1, Profile profile2)
    {
        _profile1 = profile1;
        _profile2 = profile2;
        
        RequestGameMode();

        InitializePlayers();
    }
    
    public Lobby() : this(Profile.CreateGuestProfile(), Profile.CreateGuestProfile()) { }

    public Lobby(Profile profile) : this(profile, Profile.CreateGuestProfile()) { }

    public void StartNewGame()
    {
        while (!IsGameSetEnded())
        {
            Game game = new Game(_gameMode, _player1, _player2, AreBothPlayersGuests(), player1Wins + player2Wins + 1);
        
            game.PlayGame();

            bool player1Won = game.DidPlayer1Win();
            ResolveGameEnding(player1Won);            
        }

        ResolveGameSetEnding();
    }

    private void RequestGameMode()
    {
        ConsoleKeyInfo key = InputHandler.RequestGameModeKey();
        _gameMode = key.Key switch
        {
            ConsoleKey.D1 => GameMode.PvE,
            ConsoleKey.D2 => GameMode.PvP,
            ConsoleKey.D3 => GameMode.EvE,
            _ => GameMode.PvE
        };
    }

    private void InitializePlayers()
    {
        switch (_gameMode)
        {
            case GameMode.PvP:
            {
                _player1 = CreatePlayer(ref _profile1);
                _player2 = CreatePlayer(ref _profile2);
                break;
            }
            case GameMode.PvE:
            {
                _player1 = CreatePlayer(ref _profile1);
                _player2 = new Player(false, "Player 2");
                break;
            }
            case GameMode.EvE:
            {
                _player1 = new Player(false, "Player 1");
                _player2 = new Player(false, "Player 2");
                break;
            }
        }
    }

    private Player CreatePlayer(ref Profile profile)
    {
        if (profile.IsGuest)
        {
            profile = InputHandler.RequestPlayerProfile();
        }
        
        return new Player(profile);
    }
    
    private bool AreBothPlayersGuests() => _profile1.IsGuest && _profile2.IsGuest;

    private void ResolveGameEnding(bool player1Won)
    {
        if (player1Won)
        {
            player1Wins++;
            _profile1.IncreaseVictoryScore();
            
            _profile2.IncreaseDefeatScore();
        }
        else
        {
            player2Wins++;
            _profile2.IncreaseVictoryScore();
            
            _profile1.IncreaseDefeatScore();
        }
    }

    private bool IsGameSetEnded()
    {
        return player1Wins >= maxWins || player2Wins >= maxWins;
    }

    private void ResolveGameSetEnding()
    {
        if (player1Wins >= maxWins)
            NotifyGameSetWinner(_profile1);
        else
            NotifyGameSetWinner(_profile2);
    }

    private void NotifyGameSetWinner(Profile profile)
    {
        Console.WriteLine();
        Console.WriteLine($"{profile.Name} won the best of {maxWins} with score {player1Wins}:{player2Wins}");
    }
}