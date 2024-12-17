using SeaBattle.LauncherNamespace;

namespace SeaBattle;

class Program
{
    static void Main(string[] args)
    {
        Launcher launcher = new Launcher();
        launcher.Initialize();
        launcher.LaunchNewLobby();
    }
}