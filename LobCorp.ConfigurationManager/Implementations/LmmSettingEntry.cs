// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ConfigurationManager.Config;

namespace ConfigurationManager
{
    /// <summary>
    /// Wraps an LmmConfigEntryBase for display in the configuration UI.
    /// </summary>
    public sealed class LmmSettingEntry : SettingEntryBase
    {
        /// <summary>
        /// The underlying config entry this UI wrapper displays
        /// </summary>
        public LmmConfigEntryBase Entry { get; private set; }

        /// <summary>
        /// Creates a UI wrapper for the given config entry.
        /// </summary>
        /// <param name="entry">The config entry to wrap</param>
        /// <param name="pluginInfo">Plugin metadata for grouping in the UI</param>
        /// <param name="pluginInstance">Plugin instance used for attribute discovery</param>
        public LmmSettingEntry(
            LmmConfigEntryBase entry,
            PluginInfo pluginInfo,
            object pluginInstance
        )
        {
            Entry = entry;

            DispName = entry.Definition.Key;
            Category = entry.Definition.Section;
            Description = entry.Description != null ? entry.Description.Description : null;

            var converter = ConfigConverter.GetConverter(entry.SettingType);
            if (converter != null)
            {
                ObjToStr = o => converter.ConvertToString(o, entry.SettingType);
                StrToObj = s => converter.ConvertToObject(s, entry.SettingType);
            }

            var values = entry.Description != null ? entry.Description.AcceptableValues : null;
            if (values != null)
            {
                GetAcceptableValues(values);
            }

            DefaultValue = entry.DefaultValue;

            PluginInfo = pluginInfo;
            var tags = entry.Description != null ? entry.Description.Tags : null;
            SetFromAttributes(tags, pluginInstance);
        }

        private void GetAcceptableValues(AcceptableValueBase values)
        {
            var t = values.GetType();
            var listProp = t.GetProperty(
                "AcceptableValues",
                BindingFlags.Instance | BindingFlags.Public
            );
            if (listProp != null)
            {
                AcceptableValues = ((IEnumerable)listProp.GetValue(values, null))
                    .Cast<object>()
                    .ToArray();
            }
            else
            {
                var minProp = t.GetProperty(
                    "MinValue",
                    BindingFlags.Instance | BindingFlags.Public
                );
                var maxProp = t.GetProperty(
                    "MaxValue",
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (minProp != null && maxProp != null)
                {
                    AcceptableValueRange = new System.Collections.Generic.KeyValuePair<
                        object,
                        object
                    >(minProp.GetValue(values, null), maxProp.GetValue(values, null));
                    ShowRangeAsPercent =
                        (AcceptableValueRange.Key.Equals(0) || AcceptableValueRange.Key.Equals(1))
                            && AcceptableValueRange.Value.Equals(100)
                        || AcceptableValueRange.Key.Equals(0f)
                            && AcceptableValueRange.Value.Equals(1f);
                }
            }
        }

        /// <inheritdoc />
        public override Type SettingType
        {
            get { return Entry.SettingType; }
        }

        /// <inheritdoc />
        public override object Get()
        {
            return Entry.BoxedValue;
        }

        /// <inheritdoc />
        protected override void SetValue(object newVal)
        {
            Entry.BoxedValue = newVal;
        }
    }
}
