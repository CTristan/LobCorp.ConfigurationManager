namespace ConfigurationManager.Config
{
    /// <summary>
    /// Section and key pair that uniquely identifies a setting.
    /// </summary>
    public class LmmConfigDefinition
    {
        /// <summary>
        /// INI section name used as the group header in config.cfg
        /// </summary>
        public string Section { get; private set; }

        /// <summary>
        /// Setting key within the section, unique per section
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Creates a new config definition with the given section and key.
        /// </summary>
        /// <param name="section">INI section name</param>
        /// <param name="key">Setting key within the section</param>
        public LmmConfigDefinition(string section, string key)
        {
            Section = section ?? string.Empty;
            Key = key ?? string.Empty;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as LmmConfigDefinition;
            if (other == null)
            {
                return false;
            }

            return Section == other.Section && Key == other.Key;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Section.GetHashCode() * 397) ^ Key.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Section + "." + Key;
        }
    }
}
