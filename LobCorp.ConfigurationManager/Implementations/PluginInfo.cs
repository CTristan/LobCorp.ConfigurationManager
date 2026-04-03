namespace ConfigurationManager
{
    /// <summary>
    /// Metadata about a plugin/mod, replacing BepInPlugin.
    /// </summary>
    public class PluginInfo
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }

        public PluginInfo(string guid, string name, string version)
        {
            GUID = guid ?? string.Empty;
            Name = name ?? string.Empty;
            Version = version ?? string.Empty;
        }

        public override bool Equals(object obj)
        {
            return obj is PluginInfo other && GUID == other.GUID;
        }

        public override int GetHashCode()
        {
            return GUID != null ? GUID.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return Name + " " + Version;
        }
    }
}
