// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Reflection;

namespace ConfigurationManager.Implementations
{
    /// <summary>
    /// Wraps a C# property as a setting entry for display in the configuration UI.
    /// </summary>
    public class PropertySettingEntry : SettingEntryBase
    {
        private Type _settingType;

        /// <summary>
        /// Creates a setting entry backed by the given property.
        /// </summary>
        /// <param name="instance">Object instance the property belongs to</param>
        /// <param name="settingProp">The reflected property to wrap</param>
        /// <param name="pluginInstance">Plugin instance used for attribute discovery</param>
        public PropertySettingEntry(
            object instance,
            PropertyInfo settingProp,
            object pluginInstance
        )
        {
            SetFromAttributes(settingProp.GetCustomAttributes(false), pluginInstance);
            if (Browsable == null)
            {
                Browsable = settingProp.CanRead && settingProp.CanWrite;
            }

            ReadOnly = settingProp.CanWrite;
            Property = settingProp;
            Instance = instance;
        }

        /// <summary>
        /// Object instance the property belongs to
        /// </summary>
        public object Instance { get; internal set; }

        /// <summary>
        /// The reflected PropertyInfo being wrapped
        /// </summary>
        public PropertyInfo Property { get; internal set; }

        /// <inheritdoc />
        public override string DispName
        {
            get => string.IsNullOrEmpty(base.DispName) ? Property.Name : base.DispName;
            protected internal set => base.DispName = value;
        }

        /// <inheritdoc />
        public override Type SettingType => _settingType ?? (_settingType = Property.PropertyType);

        /// <inheritdoc />
        public override object Get()
        {
            return Property.GetValue(Instance, null);
        }

        /// <inheritdoc />
        protected override void SetValue(object newVal)
        {
            Property.SetValue(Instance, newVal, null);
        }
    }
}
