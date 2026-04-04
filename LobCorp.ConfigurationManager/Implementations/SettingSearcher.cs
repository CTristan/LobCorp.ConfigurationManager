// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ConfigurationManager.Config;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.Implementations
{
    [ExcludeFromCodeCoverage(
        Justification = "Depends on Unity Application.dataPath and file system scanning"
    )]
    internal static class SettingSearcher
    {
        public static void CollectSettings(
            out IEnumerable<SettingEntryBase> results,
            out List<string> modsWithoutSettings
        )
        {
            modsWithoutSettings = new List<string>();
            results = Enumerable.Empty<SettingEntryBase>();

            // 1. Collect from LMM registration API
            try
            {
                foreach (var mod in LmmConfigRegistration.GetRegisteredMods())
                {
                    var pluginInfo = new PluginInfo(mod.ModId, mod.ModName, "1.0.0");
                    var detected = new List<SettingEntryBase>();

                    foreach (var kvp in mod.ConfigFile.Entries)
                    {
                        detected.Add(new LmmSettingEntry(kvp.Value, pluginInfo, null));
                    }

                    _ = detected.RemoveAll(x => x.Browsable == false);

                    if (detected.Count == 0)
                    {
                        modsWithoutSettings.Add(mod.ModName);
                    }
                    else
                    {
                        results = results.Concat(detected);
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Failed to collect LMM config settings: " + ex);
            }

            // 2. Auto-discover config.cfg files from BaseMods directories
            try
            {
                var baseModsPath = Path.Combine(Application.dataPath, "BaseMods");
                if (Directory.Exists(baseModsPath))
                {
                    foreach (var modDir in Directory.GetDirectories(baseModsPath))
                    {
                        var configPath = Path.Combine(modDir, "config.cfg");
                        if (!File.Exists(configPath))
                        {
                            continue;
                        }

                        var modName = Path.GetFileName(modDir);

                        // Skip if already registered via API
                        var alreadyRegistered = false;
                        foreach (var mod in LmmConfigRegistration.GetRegisteredMods())
                        {
                            if (mod.ModId == modName || mod.ConfigFile.ConfigFilePath == configPath)
                            {
                                alreadyRegistered = true;
                                break;
                            }
                        }
                        if (alreadyRegistered)
                        {
                            continue;
                        }

                        try
                        {
                            var entries = ParseConfigFile(configPath, modName);
                            if (entries.Count > 0)
                            {
                                results = results.Concat(entries);
                            }
                            else
                            {
                                modsWithoutSettings.Add(modName);
                            }
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.LogWarning(
                                "Failed to parse config file for " + modName + ": " + ex.Message
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Failed to scan BaseMods for config files: " + ex);
            }

            // 3. Optionally collect BepInEx plugins via reflection
            try
            {
                var bepInExSettings = BepInExInterop.CollectBepInExSettings();
                if (bepInExSettings != null)
                {
                    results = results.Concat(bepInExSettings);
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogWarning("BepInEx interop failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Parse a config.cfg file and create read-only setting entries from it.
        /// </summary>
        /// <param name="configPath">Absolute path to the config.cfg file to parse.</param>
        /// <param name="modName">Mod display name used to identify settings from this file.</param>
        private static List<SettingEntryBase> ParseConfigFile(string configPath, string modName)
        {
            var entries = new List<SettingEntryBase>();
            var pluginInfo = new PluginInfo(modName, modName, "");

            // Create a temporary config file and read values
            var configFile = new LmmConfigFile(configPath);

            var lines = File.ReadAllLines(configPath, System.Text.Encoding.UTF8);
            string currentSection = null;
            string currentDescription = null;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                string currentTypeName;
                string currentDefault;
                if (
                    line.StartsWith("[", StringComparison.Ordinal)
                    && line.EndsWith("]", StringComparison.Ordinal)
                )
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    currentDescription = null;
                    currentTypeName = null;
                    currentDefault = null;
                    continue;
                }

                if (line.StartsWith("## Setting type: ", StringComparison.Ordinal))
                {
                    currentTypeName = line.Substring("## Setting type: ".Length).Trim();
                    continue;
                }

                if (line.StartsWith("## Default value: ", StringComparison.Ordinal))
                {
                    currentDefault = line.Substring("## Default value: ".Length).Trim();
                    continue;
                }

                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    currentDescription = line.Substring(3).Trim();
                    continue;
                }

                if (line.StartsWith("#", StringComparison.Ordinal) || string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (currentSection == null)
                {
                    continue;
                }

                var eqIndex = line.IndexOf('=');
                if (eqIndex < 0)
                {
                    continue;
                }

                var key = line.Substring(0, eqIndex).Trim();
                var value = line.Substring(eqIndex + 1).Trim();

                // Bind as string by default for auto-discovered entries
                var entry = configFile.Bind(
                    currentSection,
                    key,
                    value,
                    currentDescription ?? string.Empty
                );
                var settingEntry = new LmmSettingEntry(entry, pluginInfo, null);
                entries.Add(settingEntry);

                currentDescription = null;
                currentTypeName = null;
                currentDefault = null;
            }

            return entries;
        }
    }
}
