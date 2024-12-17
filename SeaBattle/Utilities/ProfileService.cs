using SeaBattle.LobbyNamespace;

namespace SeaBattle.Utilities;

public class ProfileService
{
    private List<Profile> profiles;

    public void LoadProfiles()
    {
        profiles = StorageService.LoadProfiles();
    }

    public void DisplayProfiles()
    {
        Console.WriteLine("Available profiles");
        DisplayBotProfiles();
        DisplayCreateProfile();
        for (int i = 0; i < profiles.Count; i++)
        {
            DisplayProfile(i + 1, profiles[i]);
        }
        DisplayGuestProfile();
    }

    private void DisplayBotProfiles()
    {
        
    }

    private void DisplayCreateProfile()
    {
        Console.WriteLine("0) Create new profile");
    }

    private void DisplayProfile(int i, Profile profile)
    {
        Console.WriteLine(i + ") " + profile.ToString());
    }

    private void DisplayGuestProfile()
    {
        Console.WriteLine((profiles.Count + 1) + ") Play as guest");
    }

    public Profile ChooseProfile(string player)
    {
        Console.WriteLine(player + " choose a profile to play");
        int index;
        bool validIndex = true;
        do
        {
            if (!validIndex)
                Console.WriteLine($"Invalid index. Make sure it's {-4} < index < {profiles.Count + 2}.");
            
            index = InputHandler.RequestProfileIndex();
            validIndex = index > -4 && index < profiles.Count + 2;
        } while (!validIndex);

        if (index == 0)
            return CreateNewProfile();
        
        if (index < 0)
            return ChooseBot(index);

        if (index == profiles.Count + 1)
            return Profile.CreateGuestProfile();

        return ChooseExistingProfile(index);
    }

    public void UpdateProfile(LobbyMember member)
    {
        if (!member.Profile.IsGuest && member.Player.IsHuman)
        {
            int index = profiles.IndexOf(member.Profile);
            profiles[index] = member.Profile;
            StorageService.SaveProfiles(profiles);   
        }
    }

    private Profile CreateNewProfile()
    {
        Console.WriteLine("Write name of the new profile:");
        string name;
        bool available = true;
        do
        {
            name = Console.ReadLine();
            available = IsNameAvailable(name);
            if (!available)
                Console.WriteLine("Unfortunately, this name is not available. Try another");
        } while (!available);

        Profile newProfile = new Profile(name);
        
        profiles.Add(newProfile);
        
        return newProfile;
    }

    private bool IsNameAvailable(string name)
    {
        foreach (var profile in profiles)
        {
            if (profile.Name == name)
                return false;
        }

        return true;
    }

    private Profile ChooseBot(int i)
    {
        return Profile.CreateGuestProfile();
    }

    private Profile ChooseExistingProfile(int i)
    {
        return profiles[i - 1];
    }
}