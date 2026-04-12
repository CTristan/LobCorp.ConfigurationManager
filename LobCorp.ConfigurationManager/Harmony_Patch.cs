// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Diagnostics.CodeAnalysis;
using ConfigurationManager.Implementations;
using Harmony;
using LobotomyCorporation.Mods.Common.Implementations;

namespace ConfigurationManager
{
    /// <summary>
    /// LMM entry point for the Configuration Manager mod.
    /// Loaded by Basemod from LobotomyCorp_Data/BaseMods/ConfigurationManager/.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Harmony entry point")]
    public sealed class Harmony_Patch
    {
        /// <summary>
        /// Singleton created by the static initializer, triggers Harmony patching on first access.
        /// </summary>
        public static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="Harmony_Patch"/> class without triggering
        /// patch application. Used by the Basemod loader.
        /// </summary>
        public Harmony_Patch()
            : this(false) { }

        private Harmony_Patch(bool initialize)
        {
            if (!initialize)
            {
                return;
            }

            try
            {
                var harmony = HarmonyInstance.Create("com.lobcorp.configurationmanager");
                harmony.PatchAll(typeof(Harmony_Patch).Assembly);

                ConfigurationRegistry.SetProvider(new LmmConfigurationProvider());
            }
            catch (Exception ex)
            {
                try
                {
                    UnityEngine.Debug.LogError(
                        "[ConfigurationManager] Failed to apply Harmony patches: " + ex
                    );
                }
                catch (Exception)
                {
                    // Unity logging is unavailable; no alternative output target exists
                    _ = ex;
                }
            }
        }
    }
}
