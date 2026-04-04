// SPDX-License-Identifier: LGPL-3.0-or-later

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

        /// <inheritdoc />
        public override Type SettingType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// The typed setting value; triggers auto-save and change event on set
        /// </summary>
        public T Value
        {
            get { return _value; }
            set
            {
                if (Equals(_value, value))
                {
                    return;
                }

                _value = value;
                OnSettingChanged();
                if (ConfigFile != null)
                {
                    ConfigFile.Save();
                }
            }
        }

        /// <inheritdoc />
        public override object BoxedValue
        {
            get { return _value; }
            set { Value = (T)value; }
        }
    }
}
