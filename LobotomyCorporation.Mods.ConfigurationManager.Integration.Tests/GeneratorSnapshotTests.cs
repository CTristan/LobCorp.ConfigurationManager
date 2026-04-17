// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration.Tests
{
    public sealed class GeneratorSnapshotTests
    {
        [Fact]
        public Task EmitsExpectedSources_ForSimpleBinding()
        {
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                [assembly: ConfigManagerMod(ModId = "com.test.mod", ModName = "Test Mod", ModVersion = "1.0.0")]

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        public static readonly IConfigValue<int> Damage = Config.Bind("Combat", "Damage", 100);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            return Verifier.Verify(result, TestHelper.VerifySettings);
        }

        [Fact]
        public Task EmitsExpectedSources_ForMultipleBindingsAcrossTypes()
        {
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                [assembly: ConfigManagerMod(ModId = "com.test.mod", ModName = "Test Mod")]

                namespace TestMod
                {
                    internal static class Combat
                    {
                        public static readonly IConfigValue<int> Damage = Config.Bind("Combat", "Damage", 100, minValue: 1, maxValue: 1000);
                    }

                    internal static class Cheats
                    {
                        public static readonly IConfigValue<bool> GodMode = Config.Bind("Cheats", "GodMode", false, isAdvanced: true);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            return Verifier.Verify(result, TestHelper.VerifySettings);
        }

        [Fact]
        public Task EmitsRegistration_WhenModAttributeMissing()
        {
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        public static readonly IConfigValue<int> Damage = Config.Bind("Combat", "Damage", 100);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            return Verifier.Verify(result, TestHelper.VerifySettings);
        }
    }
}
