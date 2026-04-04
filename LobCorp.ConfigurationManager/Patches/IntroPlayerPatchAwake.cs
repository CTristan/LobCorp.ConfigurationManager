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
