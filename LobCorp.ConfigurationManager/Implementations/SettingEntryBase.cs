// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ConfigurationManager.Config;
using ConfigurationManager.Utilities;

namespace ConfigurationManager
{
    /// <summary>
    /// Class representing all data about a setting collected by ConfigurationManager.
    /// </summary>
    public abstract class SettingEntryBase
    {
        /// <summary>
        /// List of values this setting can take
        /// </summary>
        public object[] AcceptableValues { get; protected set; }

        /// <summary>
        /// Range of the values this setting can take
        /// </summary>
        public KeyValuePair<object, object> AcceptableValueRange { get; protected set; }

        /// <summary>
        /// Should the setting be shown as a percentage (only applies to value range settings)
        /// </summary>
        public bool? ShowRangeAsPercent { get; protected set; }

        /// <summary>
        /// Custom setting draw action.
        /// </summary>
        public Action<LmmConfigEntryBase> CustomDrawer { get; private set; }

        /// <summary>
        /// Custom setting draw action that allows polling keyboard input.
        /// </summary>
        public CustomHotkeyDrawerFunc CustomHotkeyDrawer { get; private set; }

        /// <summary>
        /// Custom hotkey drawer delegate.
        /// </summary>
        public delegate void CustomHotkeyDrawerFunc(LmmConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);

        /// <summary>
        /// Show this setting in the settings screen at all?
        /// </summary>
        public bool? Browsable { get; protected set; }

        /// <summary>
        /// Category the setting is under.
        /// </summary>
        public string Category { get; protected set; }

        /// <summary>
        /// If set, a "Default" button will be shown next to the setting.
        /// </summary>
        public object DefaultValue { get; protected set; }

        /// <summary>
        /// Force the "Reset" button to not be displayed.
        /// </summary>
        public bool HideDefaultButton { get; protected set; }

        /// <summary>
        /// Force the setting name to not be displayed.
        /// </summary>
        public bool HideSettingName { get; protected set; }

        /// <summary>
        /// Optional description shown when hovering over the setting.
        /// </summary>
        public string Description { get; protected internal set; }

        /// <summary>
        /// Name of the setting.
        /// </summary>
        public virtual string DispName { get; protected internal set; }

        /// <summary>
        /// Plugin this setting belongs to.
        /// </summary>
        public PluginInfo PluginInfo { get; protected internal set; }

        /// <summary>
        /// Only allow showing of the value.
        /// </summary>
        public bool? ReadOnly { get; protected set; }

        /// <summary>
        /// Type of the variable.
        /// </summary>
        public abstract Type SettingType { get; }

        /// <summary>
        /// Instance of the plugin/mod that owns this setting.
        /// </summary>
        public object PluginInstance { get; private set; }

        /// <summary>
        /// Is this setting advanced.
        /// </summary>
        public bool? IsAdvanced { get; internal set; }

        /// <summary>
        /// Order of the setting on the settings list.
        /// </summary>
        public int Order { get; protected set; }

        /// <summary>
        /// Get the value of this setting.
        /// </summary>
        public abstract object Get();

        /// <summary>
        /// Set the value of this setting.
        /// </summary>
        public void Set(object newVal)
        {
            if (ReadOnly != true)
                SetValue(newVal);
        }

        /// <summary>
        /// Implementation of Set.
        /// </summary>
        protected abstract void SetValue(object newVal);

        /// <summary>
        /// Custom converter from setting type to string.
        /// </summary>
        public Func<object, string> ObjToStr { get; internal set; }

        /// <summary>
        /// Custom converter from string to setting type.
        /// </summary>
        public Func<string, object> StrToObj { get; internal set; }

        private static readonly PropertyInfo[] _myProperties = typeof(SettingEntryBase).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        internal void SetFromAttributes(object[] attribs, object pluginInstance)
        {
            PluginInstance = pluginInstance;

            if (attribs == null || attribs.Length == 0) return;

            foreach (var attrib in attribs)
            {
                switch (attrib)
                {
                    case null: break;

                    case DisplayNameAttribute da:
                        DispName = da.DisplayName;
                        break;
                    case CategoryAttribute ca:
                        Category = ca.Category;
                        break;
                    case DescriptionAttribute de:
                        Description = de.Description;
                        break;
                    case DefaultValueAttribute def:
                        DefaultValue = def.Value;
                        break;
                    case ReadOnlyAttribute ro:
                        ReadOnly = ro.IsReadOnly;
                        break;
                    case BrowsableAttribute bro:
                        Browsable = bro.Browsable;
                        break;

                    case Action<SettingEntryBase> newCustomDraw:
                        CustomDrawer = _ => newCustomDraw(this);
                        break;
                    case string str:
                        switch (str)
                        {
                            case "ReadOnly": ReadOnly = true; break;
                            case "Browsable": Browsable = true; break;
                            case "Unbrowsable": case "Hidden": Browsable = false; break;
                            case "Advanced": IsAdvanced = true; break;
                        }
                        break;

                    default:
                        var attrType = attrib.GetType();
                        if (attrType.Name == "ConfigurationManagerAttributes")
                        {
                            var otherFields = attrType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                            foreach (var propertyPair in _myProperties.Join(otherFields, my => my.Name, other => other.Name, (my, other) => new { my, other }))
                            {
                                try
                                {
                                    var val = propertyPair.other.GetValue(attrib);
                                    if (val != null)
                                    {
                                        if (propertyPair.my.PropertyType != propertyPair.other.FieldType && typeof(Delegate).IsAssignableFrom(propertyPair.my.PropertyType))
                                            val = Delegate.CreateDelegate(propertyPair.my.PropertyType, ((Delegate)val).Target, ((Delegate)val).Method);

                                        propertyPair.my.SetValue(this, val, null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SimpleLogger.LogWarning("Failed to copy value " + propertyPair.my.Name + " from provided tag object " + attrType.FullName + " - " + ex.Message);
                                }
                            }
                            break;
                        }
                        return;
                }
            }
        }
    }
}
