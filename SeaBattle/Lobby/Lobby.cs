using SeaBattle.GameNamespace;
using SeaBattle.Utilities;

namespace SeaBattle.LobbyNamespace;

public struct LobbyMember
{
    public Profile Profile;
    public Player Player;
    public int WinsAmount;
}

public class Lobby
{
    private ProfileService _profileService;

    private List<Profile> chosenProfiles = new List<Profile>();
    
    private LobbyMember member1;
    private LobbyMember member2;
    
    private int maxWins = 3;

    private GameMode _gameMode;

    public Lobby(ProfileService profileService)
    {
        _profileService = profileService;
    }

    private LobbyMember CreateLobbyMember(Profile profile)
    {
        return new LobbyMember() {Profile = profile, WinsAmount = 0};
    }

    public void InitNewGame()
    {
        RequestGameMode();
        
        InitLobbyMembers();

        InitializePlayers();
    }

    public void StartNewGame()
    {
        while (!IsGameSetEnded())
        {
            Game game = new Game(_gameMode, member1, member2, AreBothPlayersGuests(), member1.WinsAmount + member2.WinsAmount + 1);
        
            game.PlayGame();

            bool player1Won = game.DidPlayer1Win();
            ResolveGameEnding(player1Won);            
        }

        ResolveGameSetEnding();
    }

    private void InitLobbyMembers()
    {
        _profileService.DisplayProfiles();

        member1 = CreateLobbyMember(Profile.CreateGuestProfile());
        member2 = CreateLobbyMember(Profile.CreateGuestProfile());
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
                member1.Player = CreatePlayer("Player 1", ref member1.Profile);
                member2.Player = CreatePlayer("Player 2", ref member2.Profile);
                break;
            }
            case GameMode.PvE:
            {
                member1.Player = CreatePlayer("Player 1", ref member1.Profile);
                member2.Player = CreateBot("Player 2");
                break;
            }
            case GameMode.EvE:
            {
                member1.Player = CreateBot("Player 1");
                member2.Player = CreateBot("Player 2");
                break;
            }
        }
    }

    //може тут не треба ref, але вирішу потім
    private Player CreatePlayer(string name, ref Profile profile)
    {
        if (profile.IsGuest)
        {
            bool isChosen = false;
            do
            {
                profile = _profileService.ChooseProfile(name);
                isChosen = chosenProfiles.Contains(profile);
                if (isChosen)
                    Console.WriteLine("You cannot choose this profile because it is already chosen by another player.");
            } while (isChosen);

            chosenProfiles.Add(profile);            
        }
        
        return new Player(profile);
    }

    private Player CreateBot(string name)
    {
        // in future different bot behaviours
        return new Player(false, name);
    }
    
    private bool AreBothPlayersGuests() => member1.Profile.IsGuest && member2.Profile.IsGuest;

    private void ResolveGameEnding(bool player1Won)
    {
        if (player1Won)
        {
            member1.WinsAmount++;
            member1.Profile.IncreaseVictoryScore();
            
            member2.Profile.IncreaseDefeatScore();
        }
        else
        {
            member2.WinsAmount++;
            member2.Profile.IncreaseVictoryScore();
            
            member1.Profile.IncreaseDefeatScore();
        }
        
        _profileService.UpdateProfile(member1);
        _profileService.UpdateProfile(member2);

        InputHandler.WaitForInput();
    }

    private bool IsGameSetEnded()
    {
        return member1.WinsAmount >= maxWins || member2.WinsAmount >= maxWins;
    }

    private void ResolveGameSetEnding()
    {
        if (member1.WinsAmount >= maxWins)
            NotifyGameSetWinner(member1);
        else
            NotifyGameSetWinner(member2);

        UpdateProfileSaves();
    }

    private void UpdateProfileSaves()
    {
        UpdateProfileSave(member1);
        UpdateProfileSave(member2);
    }

    private void UpdateProfileSave(LobbyMember member)
    {
        if (member.Profile.IsGuest)
            return;

        if (!member.Player.IsHuman)
            return;

        _profileService.UpdateProfile(member);
    }

    private void NotifyGameSetWinner(LobbyMember member)
    {
        Console.WriteLine();
        Console.WriteLine($"{member.Profile.Name} won the best of {maxWins} with score {member1.WinsAmount}:{member2.WinsAmount}");
    }
}