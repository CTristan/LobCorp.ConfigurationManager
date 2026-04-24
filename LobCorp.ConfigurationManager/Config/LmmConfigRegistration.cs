// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Static API for LMM mod authors to register config settings.
    /// </summary>
    [ExcludeFromCodeCoverage(
        Justification = "Depends on Unity Application.dataPath and static state"
    )]
    public static class LmmConfigRegistration
    {
        /// <summary>
        /// Version of the registration contract between this runtime and the
        /// LobotomyCorporation.Mods.ConfigurationManager.Integration source generator.
        /// The generator probes this constant at runtime and refuses to register against
        /// a mismatched value, so that breaking changes to the registration surface fail
        /// loudly instead of silently corrupting bindings. Bump only when the shape of
        /// the <c>Register</c> overloads changes in a way incompatible with older generators.
        /// </summary>
        public const int ApiVersion = 1;

        private static readonly Dictionary<string, RegisteredMod> RegisteredMods =
            new Dictionary<string, RegisteredMod>();

        /// <summary>
        /// Root directory for mod config files, matching LMM's persistent data location.
        /// </summary>
        internal static string ConfigBasePath =>
            Path.Combine(Application.persistentDataPath, "LobotomyBaseMod");

        /// <summary>
        /// Get or create a config file for a mod.
        /// </summary>
        /// <param name="modId">Unique mod identifier, used as the subdirectory name.</param>
        /// <param name="modName">Human-readable mod name for display in the settings UI.</param>
        /// <param name="modVersion">Optional version string shown alongside the mod name.</param>
        public static LmmConfigFile GetConfigFile(
            string modId,
            string modName,
            string modVersion = ""
        )
        {
            if (RegisteredMods.TryGetValue(modId, out var mod))
            {
                return mod.ConfigFile;
            }

            var configPath = Path.Combine(Path.Combine(ConfigBasePath, modId), "config.cfg");

            var configFile = new LmmConfigFile(configPath);
            mod = new RegisteredMod
            {
                ModId = modId,
                ModName = modName,
                ModVersion = modVersion ?? string.Empty,
                ConfigFile = configFile,
            };
            RegisteredMods[modId] = mod;

            return configFile;
        }

        /// <summary>
        /// Register a single setting for a mod using a plain description string.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="modId">Unique mod identifier.</param>
        /// <param name="modName">Human-readable mod name for display.</param>
        /// <param name="section">Config section name to group the setting under.</param>
        /// <param name="key">Setting key within the section.</param>
        /// <param name="defaultValue">Default value used when no saved value exists.</param>
        /// <param name="description">Optional plain-text description shown in the settings UI.</param>
        /// <param name="modVersion">Optional version string shown in the settings UI.</param>
        public static LmmConfigEntry<T> Register<T>(
            string modId,
            string modName,
            string section,
            string key,
            T defaultValue,
            string description = null,
            string modVersion = ""
        )
        {
            var configFile = GetConfigFile(modId, modName, modVersion);
            return configFile.Bind(section, key, defaultValue, description);
        }

        /// <summary>
        /// Register a single setting for a mod with a full description including
        /// acceptable-value constraints and UI-hint tags.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="modId">Unique mod identifier.</param>
        /// <param name="modName">Human-readable mod name for display.</param>
        /// <param name="section">Config section name to group the setting under.</param>
        /// <param name="key">Setting key within the section.</param>
        /// <param name="defaultValue">Default value used when no saved value exists.</param>
        /// <param name="description">Description carrying optional UI-hint tags and acceptable-value constraints.</param>
        /// <param name="modVersion">Optional version string shown in the settings UI.</param>
        public static LmmConfigEntry<T> Register<T>(
            string modId,
            string modName,
            string section,
            string key,
            T defaultValue,
            LmmConfigDescription description,
            string modVersion = ""
        )
        {
            var configFile = GetConfigFile(modId, modName, modVersion);
            return configFile.Bind(section, key, defaultValue, description);
        }

        /// <summary>
        /// Get all registered mods and their configs.
        /// </summary>
        internal static IEnumerable<RegisteredMod> GetRegisteredMods()
        {
            return RegisteredMods.Values;
        }

        internal sealed class RegisteredMod
        {
            public string ModId;
            public string ModName;
            public string ModVersion;
            public LmmConfigFile ConfigFile;
        }
    }
}
