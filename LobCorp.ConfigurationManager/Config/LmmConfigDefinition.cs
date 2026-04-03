namespace ConfigurationManager.Config
{
    /// <summary>
    /// Section and key pair that uniquely identifies a setting.
    /// </summary>
    public class LmmConfigDefinition
    {
        public string Section { get; private set; }
        public string Key { get; private set; }

        public LmmConfigDefinition(string section, string key)
        {
            Section = section ?? string.Empty;
            Key = key ?? string.Empty;
        }

        public override bool Equals(object obj)
        {
            var other = obj as LmmConfigDefinition;
            if (other == null) return false;
            return Section == other.Section && Key == other.Key;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Section.GetHashCode() * 397) ^ Key.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Section + "." + Key;
        }
    }
}
