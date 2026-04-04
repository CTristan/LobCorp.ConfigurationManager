namespace ConfigurationManager
{
    /// <summary>
    /// Metadata about a plugin/mod, replacing BepInPlugin.
    /// </summary>
    public class PluginInfo
    {
        /// <summary>
        /// Unique identifier for the plugin, used for equality comparison
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Display name shown in the config UI header
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version string shown alongside the plugin name
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Creates a new plugin info with the given identity.
        /// </summary>
        /// <param name="guid">Unique identifier for the plugin</param>
        /// <param name="name">Display name</param>
        /// <param name="version">Version string</param>
        public PluginInfo(string guid, string name, string version)
        {
            GUID = guid ?? string.Empty;
            Name = name ?? string.Empty;
            Version = version ?? string.Empty;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is PluginInfo other && GUID == other.GUID;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GUID != null ? GUID.GetHashCode() : 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name + " " + Version;
        }
    }
}
