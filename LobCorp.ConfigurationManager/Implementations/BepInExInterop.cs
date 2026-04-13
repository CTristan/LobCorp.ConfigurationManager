// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ConfigurationManager.Utilities;

namespace ConfigurationManager.Implementations
{
    /// <summary>
    /// Pure reflection-based interop with BepInEx.
    /// All access is guarded so the mod works without BepInEx installed.
    /// </summary>
    [ExcludeFromCodeCoverage(
        Justification = "Requires BepInEx runtime for reflection-based interop"
    )]
    internal static class BepInExInterop
    {
        private static bool _initialized;
        private static bool _bepInExAvailable;

        private static PropertyInfo _pluginInfosProperty;
        private static PropertyInfo _pluginInfoInstanceProperty;
        private static PropertyInfo _pluginInfoMetadataProperty;
        private static PropertyInfo _bepInPluginGuidProperty;
        private static PropertyInfo _bepInPluginNameProperty;
        private static PropertyInfo _bepInPluginVersionProperty;

        private static PropertyInfo _configProperty;

        private static PropertyInfo _configEntryDefinitionProperty;
        private static PropertyInfo _configEntrySettingTypeProperty;
        private static PropertyInfo _configEntryDefaultValueProperty;
        private static PropertyInfo _configEntryDescriptionProperty;

        private static PropertyInfo _configDefinitionSectionProperty;
        private static PropertyInfo _configDefinitionKeyProperty;

        private static PropertyInfo _configDescriptionDescriptionProperty;

        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            try
            {
                var bepInExAssembly = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "BepInEx");

                if (bepInExAssembly == null)
                {
                    _bepInExAvailable = false;
                    return;
                }

                var chainloaderType = bepInExAssembly.GetType("BepInEx.Bootstrap.Chainloader");
                if (chainloaderType == null)
                {
                    _bepInExAvailable = false;
                    return;
                }

                _pluginInfosProperty = chainloaderType.GetProperty(
                    "PluginInfos",
                    BindingFlags.Static | BindingFlags.Public
                );

                var pluginInfoType = bepInExAssembly.GetType("BepInEx.PluginInfo");
                if (pluginInfoType != null)
                {
                    _pluginInfoInstanceProperty = pluginInfoType.GetProperty("Instance");
                    _pluginInfoMetadataProperty = pluginInfoType.GetProperty("Metadata");
                }

                var bepInPluginType = bepInExAssembly.GetType("BepInEx.BepInPlugin");
                if (bepInPluginType != null)
                {
                    _bepInPluginGuidProperty = bepInPluginType.GetProperty("GUID");
                    _bepInPluginNameProperty = bepInPluginType.GetProperty("Name");
                    _bepInPluginVersionProperty = bepInPluginType.GetProperty("Version");
                }

                var baseUnityPluginType = bepInExAssembly.GetType("BepInEx.BaseUnityPlugin");
                if (baseUnityPluginType != null)
                {
                    _configProperty = baseUnityPluginType.GetProperty("Config");
                }

                var configEntryBaseType = bepInExAssembly.GetType(
                    "BepInEx.Configuration.ConfigEntryBase"
                );

                if (configEntryBaseType != null)
                {
                    _configEntryDefinitionProperty = configEntryBaseType.GetProperty("Definition");
                    _configEntrySettingTypeProperty = configEntryBaseType.GetProperty(
                        "SettingType"
                    );
                    _configEntryDefaultValueProperty = configEntryBaseType.GetProperty(
                        "DefaultValue"
                    );
                    _configEntryDescriptionProperty = configEntryBaseType.GetProperty(
                        "Description"
                    );
                }

                var configDefinitionType = bepInExAssembly.GetType(
                    "BepInEx.Configuration.ConfigDefinition"
                );
                if (configDefinitionType != null)
                {
                    _configDefinitionSectionProperty = configDefinitionType.GetProperty("Section");
                    _configDefinitionKeyProperty = configDefinitionType.GetProperty("Key");
                }

                var configDescriptionType = bepInExAssembly.GetType(
                    "BepInEx.Configuration.ConfigDescription"
                );
                if (configDescriptionType != null)
                {
                    _configDescriptionDescriptionProperty = configDescriptionType.GetProperty(
                        "Description"
                    );
                }

                _bepInExAvailable = _pluginInfosProperty != null;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogWarning("Failed to initialize BepInEx interop: " + ex.Message);
                _bepInExAvailable = false;
            }
        }

        /// <summary>
        /// Collect settings from BepInEx plugins via reflection.
        /// Returns an empty sequence if BepInEx is not available.
        /// </summary>
        public static IEnumerable<SettingEntryBase> CollectBepInExSettings()
        {
            Initialize();

            if (!_bepInExAvailable)
            {
                return Enumerable.Empty<SettingEntryBase>();
            }

            var results = new List<SettingEntryBase>();

            try
            {
                var pluginInfos = _pluginInfosProperty.GetValue(null, null);
                if (pluginInfos == null)
                {
                    return results;
                }

                // PluginInfos is Dictionary<string, PluginInfo>
                var valuesProperty = pluginInfos.GetType().GetProperty("Values");
                if (valuesProperty == null)
                {
                    return results;
                }

                foreach (
                    var pluginInfoObj in (IEnumerable)valuesProperty.GetValue(pluginInfos, null)
                )
                {
                    try
                    {
                        var instance = _pluginInfoInstanceProperty?.GetValue(pluginInfoObj, null);
                        if (instance == null)
                        {
                            continue;
                        }

                        // Get plugin metadata
                        var metadata = _pluginInfoMetadataProperty?.GetValue(pluginInfoObj, null);
                        var pluginInfo = CreatePluginInfo(metadata);

                        // Get config file
                        var config = _configProperty?.GetValue(instance, null);
                        if (config == null)
                        {
                            continue;
                        }

                        // Enumerate config entries
                        foreach (var entryObj in GetConfigEntries(config))
                        {
                            try
                            {
                                if (
                                    TryCreateBepInExSettingEntry(
                                        entryObj,
                                        pluginInfo,
                                        out var settingEntry
                                    )
                                )
                                {
                                    results.Add(settingEntry);
                                }
                            }
                            catch (Exception ex)
                            {
                                SimpleLogger.LogWarning(
                                    "Failed to read BepInEx config entry: " + ex.Message
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.LogWarning("Failed to process BepInEx plugin: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Failed to collect BepInEx settings: " + ex);
            }

            return results;
        }

        private static PluginInfo CreatePluginInfo(object metadata)
        {
            if (metadata == null)
            {
                return new PluginInfo("unknown", "Unknown Plugin", "");
            }

            var identifier =
                _bepInPluginGuidProperty != null
                    ? _bepInPluginGuidProperty.GetValue(metadata, null) as string
                    : "unknown";
            var name =
                _bepInPluginNameProperty != null
                    ? _bepInPluginNameProperty.GetValue(metadata, null) as string
                    : "Unknown";
            var version =
                _bepInPluginVersionProperty != null
                    ? (_bepInPluginVersionProperty.GetValue(metadata, null) ?? "").ToString()
                    : "";

            return new PluginInfo(identifier ?? "unknown", name ?? "Unknown", version ?? "");
        }

        private static IEnumerable<object> GetConfigEntries(object configFile)
        {
            // ConfigFile implements IEnumerable<KeyValuePair<ConfigDefinition, ConfigEntryBase>>
            var enumerableType = configFile.GetType().GetInterface("IEnumerable");
            if (enumerableType == null)
            {
                return Enumerable.Empty<object>();
            }

            var result = new List<object>();
            foreach (var kvp in (IEnumerable)configFile)
            {
                // Extract the Value from KeyValuePair
                var valueProperty = kvp.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var entry = valueProperty.GetValue(kvp, null);
                    if (entry != null)
                    {
                        result.Add(entry);
                    }
                }
            }
            return result;
        }

        private static bool TryCreateBepInExSettingEntry(
            object configEntryBase,
            PluginInfo pluginInfo,
            out SettingEntryBase settingEntry
        )
        {
            settingEntry = null;
            var definition = _configEntryDefinitionProperty?.GetValue(configEntryBase, null);
            var settingType =
                _configEntrySettingTypeProperty != null
                    ? _configEntrySettingTypeProperty.GetValue(configEntryBase, null) as Type
                    : null;
            var defaultValue = _configEntryDefaultValueProperty?.GetValue(configEntryBase, null);

            if (definition == null || settingType == null)
            {
                return false;
            }

            var section =
                _configDefinitionSectionProperty != null
                    ? _configDefinitionSectionProperty.GetValue(definition, null) as string
                    : "";
            var key =
                _configDefinitionKeyProperty != null
                    ? _configDefinitionKeyProperty.GetValue(definition, null) as string
                    : "";

            var descriptionObj = _configEntryDescriptionProperty?.GetValue(configEntryBase, null);
            var descriptionText = "";
            if (descriptionObj != null && _configDescriptionDescriptionProperty != null)
            {
                descriptionText =
                    _configDescriptionDescriptionProperty.GetValue(descriptionObj, null) as string
                    ?? "";
            }

            settingEntry = new BepInExSettingEntry(
                configEntryBase,
                settingType,
                section,
                key,
                descriptionText,
                defaultValue,
                pluginInfo
            );

            return true;
        }
    }

    /// <summary>
    /// Setting entry that wraps a BepInEx ConfigEntryBase via reflection.
    /// </summary>
    [ExcludeFromCodeCoverage(
        Justification = "Requires BepInEx runtime for reflection-based interop"
    )]
    internal sealed class BepInExSettingEntry : SettingEntryBase
    {
        private readonly object _configEntry;
        private readonly PropertyInfo _boxedValueProperty;

        public BepInExSettingEntry(
            object configEntry,
            Type settingType,
            string section,
            string key,
            string description,
            object defaultValue,
            PluginInfo pluginInfo
        )
        {
            _configEntry = configEntry;
            SettingType = settingType;
            _boxedValueProperty = configEntry.GetType().GetProperty("BoxedValue");

            DispName = key;
            Category = section;
            Description = description;
            DefaultValue = defaultValue;
            PluginInfo = pluginInfo;
        }

        public override Type SettingType { get; }

        public override object GetValue()
        {
            return _boxedValueProperty?.GetValue(_configEntry, null);
        }

        protected override void SetValue(object newVal)
        {
            _boxedValueProperty?.SetValue(_configEntry, newVal, null);
        }
    }
}
