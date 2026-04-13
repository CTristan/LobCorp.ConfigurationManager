// SPDX-License-Identifier: LGPL-3.0-or-later

using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Typed config entry that stores a value and persists it.
    /// </summary>
    /// <typeparam name="T">The type of value this entry stores.</typeparam>
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
        public override Type SettingType => typeof(T);

        /// <summary>
        /// The typed setting value; triggers auto-save and change event on set
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                value = ClampValue(value);

                if (Equals(_value, value))
                {
                    return;
                }

                _value = value;
                OnSettingChanged();
                ConfigFile?.Save();
            }
        }

        private T ClampValue(T value)
        {
            var acceptable = Description?.AcceptableValues;
            if (acceptable == null)
            {
                return value;
            }

            if (acceptable is IAcceptableValueRange range)
            {
                if (value is IComparable comparable)
                {
                    if (comparable.CompareTo(range.BoxedMinValue) < 0)
                    {
                        return (T)range.BoxedMinValue;
                    }

                    if (comparable.CompareTo(range.BoxedMaxValue) > 0)
                    {
                        return (T)range.BoxedMaxValue;
                    }
                }

                return value;
            }

            if (acceptable is IAcceptableValueList list)
            {
                foreach (var item in list.BoxedAcceptableValues)
                {
                    if (Equals(item, value))
                    {
                        return value;
                    }
                }

                return _value;
            }

            return value;
        }

        /// <inheritdoc />
        public override object BoxedValue
        {
            get => _value;
            set => Value = (T)value;
        }
    }
}
