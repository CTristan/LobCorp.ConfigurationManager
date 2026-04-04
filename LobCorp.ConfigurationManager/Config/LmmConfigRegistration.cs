using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ConfigurationManager.Utilities;
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
        private static readonly Dictionary<string, RegisteredMod> RegisteredMods =
            new Dictionary<string, RegisteredMod>();

        /// <summary>
        /// Get or create a config file for a mod.
        /// </summary>
        public static LmmConfigFile GetConfigFile(string modId, string modName)
        {
            RegisteredMod mod;
            if (RegisteredMods.TryGetValue(modId, out mod))
            {
                return mod.ConfigFile;
            }

            // Default config file path in BaseMods directory
            var baseModsPath = Path.Combine(Application.dataPath, "BaseMods");
            var configPath = Path.Combine(Path.Combine(baseModsPath, modId), "config.cfg");

            var configFile = new LmmConfigFile(configPath);
            mod = new RegisteredMod
            {
                ModId = modId,
                ModName = modName,
                ConfigFile = configFile,
            };
            RegisteredMods[modId] = mod;

            return configFile;
        }

        /// <summary>
        /// Register a single setting for a mod.
        /// </summary>
        public static LmmConfigEntry<T> Register<T>(
            string modId,
            string modName,
            string section,
            string key,
            T defaultValue,
            string description = null
        )
        {
            var configFile = GetConfigFile(modId, modName);
            return configFile.Bind(section, key, defaultValue, description);
        }

        /// <summary>
        /// Register a single setting with full description.
        /// </summary>
        public static LmmConfigEntry<T> Register<T>(
            string modId,
            string modName,
            string section,
            string key,
            T defaultValue,
            LmmConfigDescription description
        )
        {
            var configFile = GetConfigFile(modId, modName);
            return configFile.Bind(section, key, defaultValue, description);
        }

        /// <summary>
        /// Get all registered mods and their configs.
        /// </summary>
        internal static IEnumerable<RegisteredMod> GetRegisteredMods()
        {
            return RegisteredMods.Values;
        }

        internal class RegisteredMod
        {
            public string ModId;
            public string ModName;
            public LmmConfigFile ConfigFile;
        }
    }
}
