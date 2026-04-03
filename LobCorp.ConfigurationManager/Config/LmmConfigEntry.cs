using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Typed config entry that stores a value and persists it.
    /// </summary>
    public class LmmConfigEntry<T> : LmmConfigEntryBase
    {
        private T _value;

        internal LmmConfigEntry(
            LmmConfigFile configFile,
            LmmConfigDefinition definition,
            T defaultValue,
            LmmConfigDescription description
        )
            : base(configFile, definition, description)
        {
            DefaultValue = defaultValue;
            _value = defaultValue;
        }

        public override Type SettingType
        {
            get { return typeof(T); }
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (Equals(_value, value))
                    return;
                _value = value;
                OnSettingChanged();
                if (ConfigFile != null)
                    ConfigFile.Save();
            }
        }

        public override object BoxedValue
        {
            get { return _value; }
            set { Value = (T)value; }
        }
    }
}
