// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ConfigurationManager.Config;
using LobotomyCorporation.Mods.Common.Interfaces;

namespace ConfigurationManager.Implementations
{
    /// <summary>
    ///     Bridges Common's <see cref="IConfigurationEntry" /> abstractions to ConfigurationManager's
    ///     <see cref="LmmConfigFile" /> persistence and settings UI.
    /// </summary>
    public sealed class LmmConfigurationProvider : IConfigurationProvider
    {
        private static readonly MethodInfo s_bindMethod = FindBindMethod();

        private readonly Dictionary<string, LmmConfigFile> _configFiles =
            new Dictionary<string, LmmConfigFile>();

        /// <inheritdoc />
        public void LoadPersistedValues(IEnumerable<IConfigurationEntry> entries)
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
                            + ex.Message
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

        private void LoadEntry(IConfigurationEntry entry)
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

            // Sync persisted value → IConfigurationEntry
            var persistedValue = lmmEntry.BoxedValue;
            if (persistedValue?.Equals(entry.DefaultValue) == false)
            {
                entry.SetValue(persistedValue);
            }

            // Sync live UI changes → IConfigurationEntry
            lmmEntry.SettingChanged += (sender, args) => entry.SetValue(lmmEntry.BoxedValue);
        }

        private LmmConfigFile GetOrCreateConfigFile(string modId, string modName, string modVersion)
        {
            if (_configFiles.TryGetValue(modId, out var existing))
            {
                return existing;
            }

            var configFile = LmmConfigRegistration.GetConfigFile(modId, modName, modVersion);
            _configFiles[modId] = configFile;

            return configFile;
        }

        private static LmmConfigDescription BuildDescription(IConfigurationEntry entry)
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

            if (entry.Order != 0)
            {
                attrs.Order = entry.Order;
                hasAttrs = true;
            }

            if (entry.IsAdvanced)
            {
                attrs.IsAdvanced = true;
                hasAttrs = true;
            }

            if (entry.Hints != null)
            {
                foreach (var hint in entry.Hints)
                {
                    if (ApplyHint(attrs, hint.Key, hint.Value))
                    {
                        hasAttrs = true;
                    }
                }
            }

            if (hasAttrs)
            {
                tags.Add(attrs);
            }

            // Acceptable value constraints
            AcceptableValueBase acceptableValues = null;
            if (entry.AcceptableValueRange != null)
            {
                acceptableValues = CreateAcceptableValueRange(
                    entry.SettingType,
                    entry.AcceptableValueRange.Min,
                    entry.AcceptableValueRange.Max
                );
            }

            return new LmmConfigDescription(
                entry.Description ?? string.Empty,
                acceptableValues,
                tags.ToArray()
            );
        }

        private static bool ApplyHint(
            ConfigurationManagerAttributes attrs,
            string key,
            object value
        )
        {
            switch (key)
            {
                case "UseIntegerSlider":
                    attrs.UseIntegerSlider = value as bool?;
                    return true;
                default:
                    return false;
            }
        }

        private static AcceptableValueBase CreateAcceptableValueRange(
            Type settingType,
            object min,
            object max
        )
        {
            var openType = typeof(AcceptableValueRange<>);
            var closedType = openType.MakeGenericType(settingType);

            return (AcceptableValueBase)Activator.CreateInstance(closedType, new[] { min, max });
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
