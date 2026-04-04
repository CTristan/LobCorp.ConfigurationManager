// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConfigurationManager.Utilities;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// A config file that manages reading/writing settings in INI format.
    /// </summary>
    public class LmmConfigFile
    {
        private readonly Dictionary<LmmConfigDefinition, LmmConfigEntryBase> _entries =
            new Dictionary<LmmConfigDefinition, LmmConfigEntryBase>();
        private bool _disableSaving;

        /// <summary>
        /// Absolute path to the INI config file on disk
        /// </summary>
        public string ConfigFilePath { get; }

        /// <summary>
        /// Creates a new config file manager for the given path.
        /// </summary>
        /// <param name="configFilePath">Absolute path to the INI config file</param>
        public LmmConfigFile(string configFilePath)
        {
            ConfigFilePath = configFilePath;
        }

        /// <summary>
        /// Bind a config entry, creating it if it doesn't exist, or reading its value from the config file.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="section">Config section name.</param>
        /// <param name="key">Setting key within the section.</param>
        /// <param name="defaultValue">Default value used when no saved value exists.</param>
        /// <param name="description">Optional plain-text description shown in the settings UI.</param>
        public LmmConfigEntry<T> Bind<T>(
            string section,
            string key,
            T defaultValue,
            string description = null
        )
        {
            return Bind(
                section,
                key,
                defaultValue,
                new LmmConfigDescription(description ?? string.Empty)
            );
        }

        /// <summary>
        /// Bind a config entry with a full description.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="section">Config section name.</param>
        /// <param name="key">Setting key within the section.</param>
        /// <param name="defaultValue">Default value used when no saved value exists.</param>
        /// <param name="description">Full description including acceptable value constraints.</param>
        public LmmConfigEntry<T> Bind<T>(
            string section,
            string key,
            T defaultValue,
            LmmConfigDescription description
        )
        {
            var definition = new LmmConfigDefinition(section, key);

            if (_entries.TryGetValue(definition, out var existing))
            {
                return (LmmConfigEntry<T>)existing;
            }

            var entry = new LmmConfigEntry<T>(this, definition, defaultValue, description);
            _entries[definition] = entry;

            // Try to load saved value
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var savedValue = ReadValueFromFile(section, key);
                    if (savedValue != null)
                    {
                        var converter = ConfigConverter.GetConverter(typeof(T));
                        if (converter != null)
                        {
                            _disableSaving = true;
                            try
                            {
                                entry.Value = (T)converter.ConvertToObject(savedValue, typeof(T));
                            }
                            finally
                            {
                                _disableSaving = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SimpleLogger.LogWarning(
                        "Failed to read config value for " + definition + ": " + ex.Message
                    );
                }
            }

            return entry;
        }

        /// <summary>
        /// Get all registered entries.
        /// </summary>
        public IEnumerable<KeyValuePair<LmmConfigDefinition, LmmConfigEntryBase>> GetEntries()
        {
            return _entries;
        }

        /// <summary>
        /// Save all settings to disk.
        /// </summary>
        public void Save()
        {
            if (_disableSaving)
            {
                return;
            }

            try
            {
                var dir = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    _ = Directory.CreateDirectory(dir);
                }

                var sb = new StringBuilder();
                _ = sb.AppendLine("## Settings file");
                _ = sb.AppendLine();

                string currentSection = null;

                foreach (var kvp in _entries)
                {
                    var def = kvp.Key;
                    var entry = kvp.Value;

                    if (def.Section != currentSection)
                    {
                        if (currentSection != null)
                        {
                            _ = sb.AppendLine();
                        }

                        _ = sb.AppendLine("[" + def.Section + "]");
                        _ = sb.AppendLine();
                        currentSection = def.Section;
                    }

                    if (
                        entry.Description != null
                        && !string.IsNullOrEmpty(entry.Description.Description)
                    )
                    {
                        _ = sb.AppendLine("## " + entry.Description.Description);
                    }

                    _ = sb.AppendLine("## Setting type: " + entry.SettingType.Name);
                    _ = sb.AppendLine(
                        "## Default value: "
                            + ConfigConverter.ConvertToString(entry.DefaultValue, entry.SettingType)
                    );

                    var value = ConfigConverter.ConvertToString(
                        entry.BoxedValue,
                        entry.SettingType
                    );
                    _ = sb.AppendLine(def.Key + " = " + value);
                    _ = sb.AppendLine();
                }

                File.WriteAllText(ConfigFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Failed to save config file: " + ex.Message);
            }
        }

        /// <summary>
        /// Reload all settings from disk.
        /// </summary>
        public void Reload()
        {
            if (!File.Exists(ConfigFilePath))
            {
                return;
            }

            _disableSaving = true;
            try
            {
                foreach (var kvp in _entries)
                {
                    var savedValue = ReadValueFromFile(kvp.Key.Section, kvp.Key.Key);
                    if (savedValue != null)
                    {
                        var converter = ConfigConverter.GetConverter(kvp.Value.SettingType);
                        if (converter != null)
                        {
                            kvp.Value.BoxedValue = converter.ConvertToObject(
                                savedValue,
                                kvp.Value.SettingType
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Failed to reload config file: " + ex.Message);
            }
            finally
            {
                _disableSaving = false;
            }
        }

        private string ReadValueFromFile(string section, string key)
        {
            if (!File.Exists(ConfigFilePath))
            {
                return null;
            }

            var lines = File.ReadAllLines(ConfigFilePath, Encoding.UTF8);
            string currentSection = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (
                    line.StartsWith("[", StringComparison.Ordinal)
                    && line.EndsWith("]", StringComparison.Ordinal)
                )
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    continue;
                }

                if (line.StartsWith("#", StringComparison.Ordinal) || string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (currentSection != section)
                {
                    continue;
                }

                var eqIndex = line.IndexOf('=');
                if (eqIndex < 0)
                {
                    continue;
                }

                var lineKey = line.Substring(0, eqIndex).Trim();
                if (lineKey == key)
                {
                    return line.Substring(eqIndex + 1).Trim();
                }
            }

            return null;
        }
    }
}
