using System;
using Harmony;
using UnityEngine;

namespace ConfigurationManager.Patches
{
    /// <summary>
    /// Harmony patch that injects the ConfigManagerBehaviour MonoBehaviour
    /// into the game during IntroPlayer.Awake, following the same pattern
    /// as the DebugPanel mod.
    /// </summary>
    [HarmonyPatch(typeof(IntroPlayer), "Awake")]
    public static class IntroPlayerPatchAwake
    {
        public static void Postfix()
        {
            try
            {
                var existing = UnityEngine.Object.FindObjectOfType<ConfigManagerBehaviour>();
                if (existing != null)
                    return;

                var go = new GameObject("LobCorp_ConfigurationManager");
                go.AddComponent<ConfigManagerBehaviour>();
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
