namespace ModsManifestCreatorConsole;

public record ModsManifest(int ManifestVersion, string ServerName, string AcceptClientMod, string AcceptClientPack, 
    string NotSupportedMessage, string Timestamp, bool ForceMods, bool ForceConfigs, bool ForceResources)
{
    public List<ModEntry> Mods = new();
    public List<ConfigEntry> Configs = new();
    public List<ResourceEntry> Resources = new();

    public ModsManifest AddMod(ModEntry mod)
    {
        Mods.Add(mod);
        return this;
    }
    
    public ModsManifest AddConfig(ConfigEntry config)
    {
        Configs.Add(config);
        return this;
    }
    
    public ModsManifest AddResources(ResourceEntry resource)
    {
        Resources.Add(resource);
        return this;
    }

    public abstract class FileEntry {
        public string url;
        public string sha256;
    }

    public class ModEntry : FileEntry {
        public string fileName;

        public string modid;    // unused.
        public string version;  // unused.
    }

    public abstract class LocalRelativeFileEntry : FileEntry {
        public string localRelativePath;
    }

    public class ConfigEntry : LocalRelativeFileEntry {
    }

    public class ResourceEntry : LocalRelativeFileEntry {
        public ResourceType type;   // unused.
    }

    public enum ResourceType {
        Resource,
        Shader,
    }
}