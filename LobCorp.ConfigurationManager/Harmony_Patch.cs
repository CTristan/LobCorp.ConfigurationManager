using System;
using Harmony;

namespace ConfigurationManager
{
    /// <summary>
    /// LMM entry point for the Configuration Manager mod.
    /// Loaded by Basemod from LobotomyCorp_Data/BaseMods/ConfigurationManager/.
    /// </summary>
    public sealed class Harmony_Patch
    {
        public static readonly Harmony_Patch Instance = new Harmony_Patch();

        private Harmony_Patch()
        {
            try
            {
                var harmony = HarmonyInstance.Create("com.lobcorp.configurationmanager");
                harmony.PatchAll(typeof(Harmony_Patch).Assembly);
            }
            catch (Exception ex)
            {
                try
                {
                    UnityEngine.Debug.LogError(
                        "[ConfigurationManager] Failed to apply Harmony patches: " + ex
                    );
                }
                catch
                {
                    // Unity logging unavailable (e.g. running in unit tests)
                }
            }
        }
    }
}
