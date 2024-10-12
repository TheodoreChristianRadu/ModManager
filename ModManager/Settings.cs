using System.Text.Json;

namespace ModManager;

public static class Settings
{
    private static readonly string path = "ModManager.json";

    public static List<string> InstalledMods { get; set; } = [];
    public static List<string> AdditionalFiles { get; set; } = [];
    public static List<string> AdditionalFolders { get; set; } = [];

    public static void Load()
    {
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            InstalledMods = data.TryGetValue("InstalledMods", out var mods) ? mods : InstalledMods;
            AdditionalFiles = data.TryGetValue("AdditionalFiles", out var files) ? files : AdditionalFiles;
            AdditionalFolders = data.TryGetValue("AdditionalFolders", out var folders) ? folders : AdditionalFolders;
        }
    }

    public static void Save()
    {
        var data = new Dictionary<string, List<string>>
        {
            { "InstalledMods", InstalledMods },
            { "AdditionalFiles", AdditionalFiles },
            { "AdditionalFolders", AdditionalFolders }
        };
        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(path, json);
    }
}
