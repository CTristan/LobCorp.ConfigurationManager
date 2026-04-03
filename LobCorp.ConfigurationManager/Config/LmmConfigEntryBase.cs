using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Base class for all config entries, providing untyped access.
    /// </summary>
    public abstract class LmmConfigEntryBase
    {
        public LmmConfigDefinition Definition { get; private set; }
        public LmmConfigDescription Description { get; private set; }
        public abstract Type SettingType { get; }
        public object DefaultValue { get; protected set; }

        public abstract object BoxedValue { get; set; }

        public event EventHandler SettingChanged;

        internal LmmConfigFile ConfigFile { get; set; }

        protected LmmConfigEntryBase(LmmConfigFile configFile, LmmConfigDefinition definition, LmmConfigDescription description)
        {
            ConfigFile = configFile;
            Definition = definition;
            Description = description;
        }

        protected void OnSettingChanged()
        {
            var handler = SettingChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
