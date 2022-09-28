using System.CommandLine;
using System.Diagnostics;
using System.Security.Cryptography;
using ModsManifestCreatorConsole;
using Newtonsoft.Json;

Console.WriteLine("SyncCraft Mods Manifest Generator.");
Console.WriteLine("By: qyl27, MeowCraftMC.");
Console.WriteLine("This application is for SyncCraft 1.1.0. ");

var rootCommand = new RootCommand("SyncCraft Mods Manifest Generator.");

var serverNameOption = new Option<string>(name: "--server-name") {IsRequired = true};
var gameDirectoryOption = new Option<DirectoryInfo>(name: "--game-directory") {IsRequired = true};
var webFilePrefixOption = new Option<string>(name: "--web-file-prefix") {IsRequired = true};
var acceptPackOption = new Option<string>(aliases: new [] {"--accept-pack-version", "--pack", "-p"}, getDefaultValue: () => "(,)");
var notSupportMessageOption = new Option<string>(name: "--not-support-message", getDefaultValue: () => "客户端版本不受支持！");
var forceModsOption = new Option<bool>(name: "--force-mods", getDefaultValue: () => true);
var forceConfigOption = new Option<bool>(name: "--force-config", getDefaultValue: () => false);
var forceResourcesOption = new Option<bool>(name: "--force-resources", getDefaultValue: () => false);

rootCommand.AddOption(serverNameOption);
// rootCommand.AddOption(new Option<FileInfo>(name: "GameDirectory", parseArgument: result => new FileInfo(result.GetValueOrDefault<string>())) { IsRequired = true });
rootCommand.AddOption(gameDirectoryOption);
rootCommand.AddOption(webFilePrefixOption);
rootCommand.AddOption(acceptPackOption);
rootCommand.AddOption(notSupportMessageOption);
rootCommand.AddOption(forceModsOption);
rootCommand.AddOption(forceConfigOption);
rootCommand.AddOption(forceResourcesOption);

rootCommand.SetHandler(MakeManifest, serverNameOption, gameDirectoryOption, webFilePrefixOption, 
    acceptPackOption, notSupportMessageOption, 
    forceModsOption, forceConfigOption, forceResourcesOption);

return await rootCommand.InvokeAsync(args);

void MakeManifest(string serverName, DirectoryInfo gameDir, string webFilePrefix, string acceptClientPack, 
    string notSupportedMessage, bool forceMods, bool forceConfigs, bool forceResources)
{
    Console.WriteLine($"Server name: {serverName}.");
    
    var timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    
    var manifest = new ModsManifest(1, serverName, "(,)", acceptClientPack, 
        notSupportedMessage, timestamp.ToString(), forceMods, forceConfigs, forceResources);

    var modsPath = Path.Combine(gameDir.FullName, "mods");
    if (Directory.Exists(modsPath))
    {
        var mods = Directory.GetFiles(modsPath);
        foreach (var mod in mods)
        {
            var fileName = Path.GetFileName(mod);
            manifest.AddMod(new ModsManifest.ModEntry
            {
                fileName = fileName,
                sha256 = GetSha256(mod),
                url = webFilePrefix + "/public/mods/" + fileName
            });
            
            File.Copy(mod, Path.Combine("mods", fileName));

            Console.WriteLine($"Added mod: {fileName}.");
        }
    }
    
    var configPath = Path.Combine(gameDir.FullName, "configs");
    if (Directory.Exists(configPath))
    {
        var configs = GetFiles(modsPath);
        foreach (var config in configs)
        {
            var fileRelativePath = Path.GetRelativePath(gameDir.FullName, config).Replace("\\", "/");
            manifest.AddConfig(new ModsManifest.ConfigEntry
            {
                localRelativePath = fileRelativePath,
                sha256 = GetSha256(config),
                url = webFilePrefix + "/public/mods/" + fileRelativePath
            });
            
            File.Copy(config, Path.Combine("configs", fileRelativePath));

            Console.WriteLine($"Added config: {fileRelativePath}.");
        }
    }
    
    // Todo: Resource generate.
    
    var manifestJson = JsonConvert.SerializeObject(manifest);
    File.WriteAllText("mods.json", manifestJson);
}

string GetSha256(string path)
{
    using var hasher = HashAlgorithm.Create("SHA256");
    using var stream = File.OpenRead(path);
    var hash = hasher?.ComputeHash(stream);
    Debug.Assert(hash is not null);
    return BitConverter.ToString(hash).Replace("-", "");
}

List<string> GetFiles(string path)
{
    var files = new List<string>();
    
    var dirs = Directory.GetDirectories(path);
    foreach (var dir in dirs)
    {
        files.AddRange(GetFiles(dir));
    }

    files.AddRange(Directory.GetFiles(path));

    return files;
}

