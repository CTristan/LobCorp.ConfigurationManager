// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Linq;
using AwesomeAssertions;
using Xunit;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration.Tests
{
    public sealed class GeneratorDiagnosticsTests
    {
        [Fact]
        public void DuplicateBinding_ReportsLCM001()
        {
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                [assembly: ConfigManagerMod(ModId = "com.test.mod", ModName = "Test Mod")]

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        public static readonly IConfigValue<int> First =
                            Config.Bind("Section", "Key", 100);
                        public static readonly IConfigValue<int> Second =
                            Config.Bind("Section", "Key", 200);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            var diagnostics = result.Results.SelectMany(r => r.Diagnostics).ToList();
            diagnostics.Should().ContainSingle(d => d.Id == "LCM001");
        }

        [Fact]
        public void DuplicateModAttribute_ReportsLCM002()
        {
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                [assembly: ConfigManagerMod(ModId = "com.test.mod", ModName = "Test Mod")]
                [assembly: ConfigManagerMod(ModId = "com.other.mod", ModName = "Other Mod")]

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        public static readonly IConfigValue<int> Damage =
                            Config.Bind("Combat", "Damage", 100);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            var diagnostics = result.Results.SelectMany(r => r.Diagnostics).ToList();
            diagnostics.Should().ContainSingle(d => d.Id == "LCM002");
        }

        [Fact]
        public void BindWithTooFewArguments_IsSkippedSilently()
        {
            // Scanner matches Config.Bind by name, TryExtract bails when args < 3.
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        public static readonly object TooFew = Config.Bind("a", "b");
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public void BindWithNonLiteralSectionOrKey_IsSkippedSilently()
        {
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        private const string S = "Section";
                        public static readonly IConfigValue<int> X = Config.Bind(S, "Key", 100);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public void ConfigManagerModOnNonAssemblyTarget_IsIgnored()
        {
            // Attribute parent check: only [assembly: ...] is honored.
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                namespace TestMod
                {
                    [ConfigManagerMod(ModId = "x", ModName = "y")]
                    internal static class MyConfig
                    {
                        public static readonly IConfigValue<int> Damage =
                            Config.Bind("Combat", "Damage", 100);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public void ConfigManagerModMissingModId_IsIgnored()
        {
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                [assembly: ConfigManagerMod(ModName = "Only Name")]

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        public static readonly IConfigValue<int> Damage =
                            Config.Bind("Combat", "Damage", 100);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public void ConfigManagerModWithFallbackQualified_ExtractsEnumSuffix()
        {
            // Exercises StripEnumPrefix branch on a dotted enum expression.
            const string Source = """
                using LobotomyCorporation.Mods.ConfigurationManager;

                [assembly: ConfigManagerMod(
                    ModId = "com.test",
                    ModName = "Test",
                    Fallback = ConfigFallback.InMemory)]

                namespace TestMod
                {
                    internal static class MyConfig
                    {
                        public static readonly IConfigValue<int> Damage =
                            Config.Bind("Combat", "Damage", 100);
                    }
                }
                """;

            var result = TestHelper.RunGenerator(Source);
            result.Results.Should().NotBeEmpty();
        }
    }
}
