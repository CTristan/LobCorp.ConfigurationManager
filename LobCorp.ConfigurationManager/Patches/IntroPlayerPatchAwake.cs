// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Diagnostics.CodeAnalysis;
using ConfigurationManager.Implementations;
using Harmony;
using UnityEngine;

namespace ConfigurationManager.Patches
{
    /// <summary>
    /// Harmony patch that injects the ConfigManagerBehaviour MonoBehaviour
    /// into the game during IntroPlayer.Awake, following the same pattern
    /// as the DebugPanel mod.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Harmony entry point, tested for patch targeting")]
    [HarmonyPatch(typeof(IntroPlayer), "Awake")]
    public static class IntroPlayerPatchAwake
    {
        /// <summary>
        /// Harmony postfix that injects the ConfigManagerBehaviour into the scene
        /// </summary>
        public static void Postfix()
        {
            try
            {
                var existing = UnityEngine.Object.FindObjectOfType<ConfigManagerBehaviour>();
                if (existing != null)
                {
                    return;
                }

                if (DuplicateAssemblyDetector.IsDuplicateLoaded)
                {
                    Debug.LogError(
                        "[ConfigurationManager] Duplicate ConfigurationManager assembly detected at: "
                            + DuplicateAssemblyDetector.DuplicateLocation
                            + " — remove the original BepInEx.ConfigurationManager from BepInEx/plugins/ to avoid conflicts."
                    );
                }

                var go = new GameObject("LobCorp_ConfigurationManager");
                _ = go.AddComponent<ConfigManagerBehaviour>();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[ConfigurationManager] Failed to inject ConfigManagerBehaviour: " + ex
                );
            }
        }
    }
}
