// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using Harmony;
using LobotomyCorporation.Mods.ConfigurationManager;

namespace SampleMod
{
    /// <summary>
    /// Entry point that LMM (Lobotomy Mod Manager, the base game's mod
    /// loader) instantiates when this mod loads. Applies Harmony patches
    /// and registers our configuration bindings.
    /// </summary>
    /// <remarks>
    /// LMM loads this mod by calling <c>new Harmony_Patch()</c>
    /// reflectively. The first time any code touches the
    /// <c>Harmony_Patch</c> type, the CLR runs the static constructor
    /// below — that is where all of our setup happens. The instance LMM
    /// gets back is a no-op; the work has already been done.
    /// </remarks>
    public sealed class Harmony_Patch
    {
        static Harmony_Patch()
        {
            try
            {
                var harmony = HarmonyInstance.Create("com.example.samplemod");
                harmony.PatchAll(typeof(Harmony_Patch).Assembly);

                // Registers every Config.Bind(...) declaration in this
                // assembly with ConfigurationManager if it is installed.
                // If ConfigurationManager is absent, this call returns
                // without error and bindings keep their default values in
                // memory. You never write an "is ConfigurationManager
                // installed?" check yourself — the source generator emits
                // that for you.
                Config.RegisterAll();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[SampleMod] Harmony patch failure: " + ex);
            }
        }
    }
}
