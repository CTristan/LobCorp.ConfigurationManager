// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ConfigurationManager.Config;
using LobotomyCorporation.Mods.Common;

namespace ConfigurationManager.Implementations
{
    /// <summary>
    ///     Bridges Common's <see cref="IConfigEntry" /> abstractions to ConfigurationManager's
    ///     <see cref="LmmConfigFile" /> persistence and settings UI.
    /// </summary>
    public sealed class LmmConfigurationProvider : IConfigProvider
    {
        private static readonly MethodInfo s_bindMethod = FindBindMethod();

        private readonly Dictionary<string, LmmConfigFile> _configFiles =
            new Dictionary<string, LmmConfigFile>();

        private readonly Func<string, string, string, LmmConfigFile> _configFileFactory;

        /// <summary>
        /// Creates a provider that persists through <see cref="LmmConfigRegistration.GetConfigFile"/>.
        /// </summary>
        public LmmConfigurationProvider()
            : this(LmmConfigRegistration.GetConfigFile) { }

        internal LmmConfigurationProvider(
            Func<string, string, string, LmmConfigFile> configFileFactory
        )
        {
            _configFileFactory =
                configFileFactory ?? throw new ArgumentNullException(nameof(configFileFactory));
        }

        /// <inheritdoc />
        public void LoadPersistedValues(IEnumerable<IConfigEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            foreach (var entry in entries)
            {
                try
                {
                    LoadEntry(entry);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning(
                        "[ConfigurationManager] Failed to load entry "
                            + entry.ModId
                            + ":"
                            + entry.Section
                            + ":"
                            + entry.Key
                            + " - "
                            + ex
                    );
                }
            }
        }

        /// <inheritdoc />
        public void Save()
        {
            foreach (var configFile in _configFiles.Values)
            {
                configFile.Save();
            }
        }

        private void LoadEntry(IConfigEntry entry)
        {
            var configFile = GetOrCreateConfigFile(entry.ModId, entry.ModName, entry.ModVersion);
            var description = BuildDescription(entry);

            var genericBind = s_bindMethod.MakeGenericMethod(entry.SettingType);
            var lmmEntry = (LmmConfigEntryBase)
                genericBind.Invoke(
                    configFile,
                    new[] { entry.Section, entry.Key, entry.DefaultValue, description }
                );

            if (lmmEntry == null)
            {
                return;
            }

            // Sync persisted value → IConfigEntry
            var persistedValue = lmmEntry.BoxedValue;
            if (!object.Equals(persistedValue, entry.DefaultValue))
            {
                entry.Value = persistedValue;
            }

            // Sync live UI changes → IConfigEntry
            lmmEntry.SettingChanged += (sender, args) => entry.Value = lmmEntry.BoxedValue;
        }

        private LmmConfigFile GetOrCreateConfigFile(string modId, string modName, string modVersion)
        {
            if (_configFiles.TryGetValue(modId, out var existing))
            {
                return existing;
            }

            var configFile = _configFileFactory(modId, modName, modVersion);
            _configFiles[modId] = configFile;

            return configFile;
        }

        private static LmmConfigDescription BuildDescription(IConfigEntry entry)
        {
            var tags = new List<object>();

            // DisplayName tag
            if (!string.IsNullOrEmpty(entry.DisplayName))
            {
                tags.Add(new DisplayNameAttribute(entry.DisplayName));
            }

            // ConfigurationManagerAttributes for UI hints
            var attrs = new ConfigurationManagerAttributes();
            var hasAttrs = false;

            if (entry.UseSlider)
            {
                attrs.UseIntegerSlider = true;
                hasAttrs = true;
            }

            if (hasAttrs)
            {
                tags.Add(attrs);
            }

            // Acceptable value constraints from the generic Range property.
            // IConfigEntry (non-generic) doesn't expose Range, so we use reflection
            // to read it from the concrete IConfigEntry<T> implementation.
            IAcceptableValue acceptableValues = null;
            if (TryGetRangeViaReflection(entry, out var rangeMin, out var rangeMax))
            {
                acceptableValues = CreateAcceptableValueRange(
                    entry.SettingType,
                    rangeMin,
                    rangeMax
                );
            }

            return new LmmConfigDescription(
                entry.Description ?? string.Empty,
                acceptableValues,
                tags.ToArray()
            );
        }

        private static bool TryGetRangeViaReflection(
            IConfigEntry entry,
            out object min,
            out object max
        )
        {
            min = null;
            max = null;

            // IConfigEntry<T>.Range is AcceptableValueRange<T> with Min/Max properties.
            var entryType = entry.GetType();
            var rangeProp = entryType.GetProperty("Range");
            if (rangeProp == null)
            {
                return false;
            }

            var rangeValue = rangeProp.GetValue(entry, null);
            if (rangeValue == null)
            {
                return false;
            }

            var rangeType = rangeValue.GetType();
            var minProp = rangeType.GetProperty("Min");
            var maxProp = rangeType.GetProperty("Max");
            if (minProp == null || maxProp == null)
            {
                return false;
            }

            min = minProp.GetValue(rangeValue, null);
            max = maxProp.GetValue(rangeValue, null);

            return min != null && max != null;
        }

        private static IAcceptableValue CreateAcceptableValueRange(
            Type settingType,
            object min,
            object max
        )
        {
            var openType = typeof(Config.AcceptableValueRange<>);
            var closedType = openType.MakeGenericType(settingType);

            return (IAcceptableValue)Activator.CreateInstance(closedType, new[] { min, max });
        }

        private static MethodInfo FindBindMethod()
        {
            foreach (var method in typeof(LmmConfigFile).GetMethods())
            {
                if (method.Name == "Bind" && method.IsGenericMethodDefinition)
                {
                    var parameters = method.GetParameters();
                    if (
                        parameters.Length == 4
                        && parameters[0].ParameterType == typeof(string)
                        && parameters[1].ParameterType == typeof(string)
                        && parameters[3].ParameterType == typeof(LmmConfigDescription)
                    )
                    {
                        return method;
                    }
                }
            }

            throw new MissingMethodException(
                "Could not find LmmConfigFile.Bind<T>(string, string, T, LmmConfigDescription)"
            );
        }
    }
}
