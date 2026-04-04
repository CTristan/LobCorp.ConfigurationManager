using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Base class for all config entries, providing untyped access.
    /// </summary>
    public abstract class LmmConfigEntryBase
    {
        /// <summary>
        /// Section and key identity for this entry
        /// </summary>
        public LmmConfigDefinition Definition { get; private set; }

        /// <summary>
        /// Metadata including description text and value constraints
        /// </summary>
        public LmmConfigDescription Description { get; private set; }

        /// <summary>
        /// CLR type of the setting value
        /// </summary>
        public abstract Type SettingType { get; }

        /// <summary>
        /// Value used when resetting or before the config file is read
        /// </summary>
        public object DefaultValue { get; protected set; }

        /// <summary>
        /// Untyped access to the setting value for generic UI code
        /// </summary>
        public abstract object BoxedValue { get; set; }

        /// <summary>
        /// Raised when the value changes
        /// </summary>
        public event EventHandler SettingChanged;

        internal LmmConfigFile ConfigFile { get; set; }

        /// <summary>
        /// Initializes a new config entry with the given file, definition, and description.
        /// </summary>
        /// <param name="configFile">The config file that owns this entry</param>
        /// <param name="definition">Section and key identity</param>
        /// <param name="description">Metadata including description text and constraints</param>
        protected LmmConfigEntryBase(
            LmmConfigFile configFile,
            LmmConfigDefinition definition,
            LmmConfigDescription description
        )
        {
            ConfigFile = configFile;
            Definition = definition;
            Description = description;
        }

        /// <summary>
        /// Raises the <see cref="SettingChanged"/> event.
        /// </summary>
        protected void OnSettingChanged()
        {
            var handler = SettingChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
