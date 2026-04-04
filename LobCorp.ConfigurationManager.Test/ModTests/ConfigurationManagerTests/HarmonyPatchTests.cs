// SPDX-License-Identifier: MIT

#region

using ConfigurationManager;
using ConfigurationManager.Patches;
using LobCorp.ConfigurationManager.Test.Extensions;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void IntroPlayerPatchAwake_ShouldTargetIntroPlayerAwake()
        {
            _ = Harmony_Patch.Instance;

            typeof(IntroPlayerPatchAwake).ValidateHarmonyPatch(typeof(IntroPlayer), "Awake");
        }
    }
}
