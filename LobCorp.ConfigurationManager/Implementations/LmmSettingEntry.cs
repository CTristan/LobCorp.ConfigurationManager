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
    internal sealed class LmmSettingEntry : SettingEntryBase
    {
        public LmmConfigEntryBase Entry { get; private set; }

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

        public override Type SettingType
        {
            get { return Entry.SettingType; }
        }

        public override object Get()
        {
            return Entry.BoxedValue;
        }

        protected override void SetValue(object newVal)
        {
            Entry.BoxedValue = newVal;
        }
    }
}
