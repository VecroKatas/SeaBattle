using SeaBattle.LobbyNamespace;
using SeaBattle.Utilities;

namespace SeaBattle.LauncherNamespace;

public class Launcher
{
    private ProfileService _profileService;
    
    public Launcher(){}

    public void Initialize()
    {
        _profileService = new ProfileService();
        _profileService.LoadProfiles();
    }

    public void LaunchNewLobby()
    {
        Lobby lobby = new Lobby(_profileService);
        lobby.InitNewGame();
        lobby.StartNewGame();
    }
}