// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AwesomeAssertions;
using ConfigurationManager.Config;
using ConfigurationManager.Implementations;
using LobotomyCorporation.Mods.Common;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class LmmConfigurationProviderTests : IDisposable
    {
        private readonly List<string> _tempPaths = [];

        public void Dispose()
        {
            foreach (var path in _tempPaths)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private LmmConfigFile CreateTempConfigFile()
        {
            var path = Path.GetTempFileName();
            _tempPaths.Add(path);
            return new LmmConfigFile(path);
        }

        private LmmConfigurationProvider CreateProvider(LmmConfigFile? reusableFile = null)
        {
            var file = reusableFile ?? CreateTempConfigFile();
            return new LmmConfigurationProvider((_, _, _) => file);
        }

        [Fact]
        public void Constructor_NullFactory_ShouldThrowArgumentNullException()
        {
            Action act = () =>
            {
                var _ = new LmmConfigurationProvider(
                    (Func<string, string, string, LmmConfigFile>)null!
                );
            };

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_Default_ShouldNotThrow()
        {
            Action act = () =>
            {
                var _ = new LmmConfigurationProvider();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void LoadPersistedValues_NullEntries_ShouldThrowArgumentNullException()
        {
            var provider = CreateProvider();

            Action act = () => provider.LoadPersistedValues(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LoadPersistedValues_ShouldBindEntryToConfigFile()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
            };

            provider.LoadPersistedValues([entry]);

            var bound = file.Bind("General", "Volume", 0);
            bound.BoxedValue.Should().Be(50);
        }

        [Fact]
        public void LoadPersistedValues_PersistedValueDifferentFromDefault_ShouldSyncValue()
        {
            var file = CreateTempConfigFile();
            file.Bind("General", "Volume", 50).Value = 75;

            var provider = CreateProvider(file);
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
            };

            provider.LoadPersistedValues([entry]);

            entry.Value.Should().Be(75);
        }

        [Fact]
        public void LoadPersistedValues_PersistedValueMatchesDefault_ShouldNotChangeValue()
        {
            var provider = CreateProvider();
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
                Value = 50,
            };

            provider.LoadPersistedValues([entry]);

            // Value should still be the default — provider should not have overwritten it.
            entry.Value.Should().Be(50);
        }

        [Fact]
        public void LoadPersistedValues_ChangingBoundEntry_ShouldPropagateToConfigEntry()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
            };

            provider.LoadPersistedValues([entry]);
            var bound = file.Bind("General", "Volume", 0);
            bound.BoxedValue = 99;

            entry.Value.Should().Be(99);
        }

        [Fact]
        public void LoadPersistedValues_EntryWithDisplayName_ShouldAttachDisplayNameTag()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                DisplayName = "Volume Level",
                Description = "Controls volume",
                SettingType = typeof(int),
                DefaultValue = 50,
            };

            provider.LoadPersistedValues([entry]);

            var bound = file.Bind("General", "Volume", 0);
            bound.Description.Description.Should().Be("Controls volume");
            bound.Description.Tags.OfType<DisplayNameAttribute>().Should().ContainSingle();
        }

        [Fact]
        public void LoadPersistedValues_EntryWithUseSlider_ShouldAttachConfigurationManagerAttributes()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
                UseSlider = true,
            };

            provider.LoadPersistedValues([entry]);

            var bound = file.Bind("General", "Volume", 0);
            var attrs = bound
                .Description.Tags.OfType<global::ConfigurationManager.ConfigurationManagerAttributes>()
                .Single();
            attrs.UseIntegerSlider.Should().Be(true);
        }

        [Fact]
        public void LoadPersistedValues_EntryWithoutUseSlider_ShouldNotAttachConfigurationManagerAttributes()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
                UseSlider = false,
            };

            provider.LoadPersistedValues([entry]);

            var bound = file.Bind("General", "Volume", 0);
            bound
                .Description.Tags.OfType<global::ConfigurationManager.ConfigurationManagerAttributes>()
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void LoadPersistedValues_EntryWithAcceptableValueRange_ShouldSetAcceptableValues()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);

            // Use a real ModConfig + Bind to get a real IConfigEntry<int> with Range set.
            var config = new ModConfig("mod1", "Mod", "1.0");
            var realEntry = config.Bind(
                "General",
                "Volume",
                50,
                "Volume setting",
                range: new LobotomyCorporation.Mods.Common.AcceptableValueRange<int>(0, 100)
            );

            provider.LoadPersistedValues([realEntry]);

            var bound = file.Bind("General", "Volume", 0);
            bound
                .Description.AcceptableValues.Should()
                .BeOfType<global::ConfigurationManager.Config.AcceptableValueRange<int>>();
        }

        [Fact]
        public void GetOrCreateConfigFile_SameModId_ShouldReuseFile()
        {
            var factoryCallCount = 0;
            var file = CreateTempConfigFile();
            var provider = new LmmConfigurationProvider(
                (_, _, _) =>
                {
                    factoryCallCount++;
                    return file;
                }
            );

            var entry1 = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "A",
                SettingType = typeof(int),
                DefaultValue = 1,
            };
            var entry2 = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "B",
                SettingType = typeof(int),
                DefaultValue = 2,
            };

            provider.LoadPersistedValues([entry1, entry2]);

            factoryCallCount.Should().Be(1);
        }

        [Fact]
        public void Save_ShouldInvokeSaveOnAllRegisteredConfigFiles()
        {
            var path = Path.GetTempFileName();
            _tempPaths.Add(path);
            var file = new LmmConfigFile(path);
            var provider = CreateProvider(file);
            var entry = new StubConfigEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
            };
            provider.LoadPersistedValues([entry]);
            file.Bind("General", "Volume", 0).Value = 77;

            File.WriteAllText(path, string.Empty);

            provider.Save();

            var contents = File.ReadAllText(path);
            contents.Should().Contain("Volume = 77");
        }

        [Fact]
        public void Save_WithNoLoadedEntries_ShouldNotThrow()
        {
            var provider = CreateProvider();

            Action act = provider.Save;

            act.Should().NotThrow();
        }

        /// <summary>
        ///     Minimal stub implementing <see cref="IConfigEntry"/> for testing the provider
        ///     without needing a real <see cref="ModConfig"/>.
        /// </summary>
        private sealed class StubConfigEntry : IConfigEntry
        {
            public string ModId { get; set; } = "mod";
            public string ModName { get; set; } = "Mod";
            public string ModVersion { get; set; } = "1.0";
            public string Section { get; set; } = "Section";
            public string Key { get; set; } = "Key";
            public string DisplayName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public Type SettingType { get; set; } = typeof(int);
            public object DefaultValue { get; set; } = 0;
            public bool UseSlider { get; set; }
            public object Value { get; set; } = 0;
        }
    }
}
