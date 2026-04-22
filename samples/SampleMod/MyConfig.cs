// SPDX-License-Identifier: LGPL-3.0-or-later

using LobotomyCorporation.Mods.ConfigurationManager;

// The [assembly: ConfigManagerMod] attribute identifies your mod to the
// ConfigurationManager integration. The source generator reads these
// values at build time and uses them when registering your settings
// at runtime.
//
// Fallback = ConfigFallback.InMemory means: if a player runs your mod
// without ConfigurationManager installed, the generator's registration
// helper skips the UI step and your settings live in memory with their
// default values. Your mod still runs. Nothing crashes. No error logs.
[assembly: ConfigManagerMod(
    ModId = "com.example.samplemod",
    ModName = "Sample Mod",
    ModVersion = "0.1.0",
    Fallback = ConfigFallback.InMemory
)]

namespace SampleMod
{
    /// <summary>
    /// Three example settings showing the most common shapes of
    /// <c>Config.Bind</c>: a numeric range, a boolean flag, and a plain
    /// string. Read any binding's current value at runtime via
    /// <c>MyConfig.Damage.Value</c>, <c>MyConfig.GodMode.Value</c>, and
    /// so on.
    /// </summary>
    internal static class MyConfig
    {
        /// <summary>
        /// A numeric setting with a range. ConfigurationManager renders
        /// this as a slider because <c>minValue</c> and <c>maxValue</c>
        /// are set. <c>order</c> controls where the setting appears in
        /// the F1 menu relative to other settings in the same section
        /// (higher = earlier).
        /// </summary>
        public static readonly IConfigValue<int> Damage = Config.Bind(
            section: "Combat",
            key: "Damage",
            defaultValue: 100,
            description: "Damage per hit",
            minValue: 1,
            maxValue: 1000,
            order: 10
        );

        /// <summary>
        /// <c>isAdvanced: true</c> hides this setting behind an
        /// "Advanced" toggle in the F1 menu, so regular players do not
        /// see it by default.
        /// </summary>
        public static readonly IConfigValue<bool> GodMode = Config.Bind(
            section: "Cheats",
            key: "GodMode",
            defaultValue: false,
            isAdvanced: true
        );

        /// <summary>
        /// A plain string setting. ConfigurationManager renders a text box.
        /// </summary>
        public static readonly IConfigValue<string> PlayerName = Config.Bind(
            section: "General",
            key: "PlayerName",
            defaultValue: "Manager"
        );
    }
}
