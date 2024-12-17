using System.Text.Json;
using SeaBattle.LobbyNamespace;

namespace SeaBattle.Utilities;

public static class StorageService
{
    private static string folderPath = "Profiles/";

    public static void SaveProfiles(List<Profile> profiles)
    {
        string json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true});
        File.WriteAllText(folderPath + "Profiles.json", json);
    }
    
    public static List<Profile> LoadProfiles()
    {
        try
        {
            string json = File.ReadAllText(folderPath + "Profiles.json");
            return JsonSerializer.Deserialize<List<Profile>>(json, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true});
        }
        catch (Exception e) { }

        return new List<Profile>();
    }
}