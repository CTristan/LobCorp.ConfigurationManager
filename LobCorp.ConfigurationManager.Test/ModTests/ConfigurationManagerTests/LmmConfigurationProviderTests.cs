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
using Xunit;
using CommonAbstractions = LobotomyCorporation.Mods.Common.Implementations;
using CommonInterfaces = LobotomyCorporation.Mods.Common.Interfaces;

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
            var entry = new StubConfigurationEntry
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
        public void LoadPersistedValues_PersistedValueDifferentFromDefault_ShouldCallSetValue()
        {
            var file = CreateTempConfigFile();
            file.Bind("General", "Volume", 50).Value = 75;

            var provider = CreateProvider(file);
            var entry = new StubConfigurationEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
            };

            provider.LoadPersistedValues([entry]);

            entry.LastSetValue.Should().Be(75);
        }

        [Fact]
        public void LoadPersistedValues_PersistedValueMatchesDefault_ShouldNotCallSetValue()
        {
            var provider = CreateProvider();
            var entry = new StubConfigurationEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
            };

            provider.LoadPersistedValues([entry]);

            entry.SetValueCallCount.Should().Be(0);
        }

        [Fact]
        public void LoadPersistedValues_ChangingBoundEntry_ShouldPropagateToConfigurationEntry()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigurationEntry
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

            entry.LastSetValue.Should().Be(99);
        }

        [Fact]
        public void LoadPersistedValues_EntryWithDisplayNameAndDescription_ShouldAttachTagsToConfigEntry()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigurationEntry
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
        public void LoadPersistedValues_EntryWithOrderAndAdvanced_ShouldAttachConfigurationManagerAttributes()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigurationEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
                Order = 10,
                IsAdvanced = true,
            };

            provider.LoadPersistedValues([entry]);

            var bound = file.Bind("General", "Volume", 0);
            var attrs = bound
                .Description.Tags.OfType<global::ConfigurationManager.ConfigurationManagerAttributes>()
                .Single();
            attrs.Order.Should().Be(10);
            attrs.IsAdvanced.Should().Be(true);
        }

        [Fact]
        public void LoadPersistedValues_EntryWithKnownHint_ShouldApplyHint()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigurationEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
                Hints = new Dictionary<string, object> { { "UseIntegerSlider", true } },
            };

            provider.LoadPersistedValues([entry]);

            var bound = file.Bind("General", "Volume", 0);
            var attrs = bound
                .Description.Tags.OfType<global::ConfigurationManager.ConfigurationManagerAttributes>()
                .Single();
            attrs.UseIntegerSlider.Should().Be(true);
        }

        [Fact]
        public void LoadPersistedValues_EntryWithUnknownHint_ShouldIgnoreHint()
        {
            var file = CreateTempConfigFile();
            var provider = CreateProvider(file);
            var entry = new StubConfigurationEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
                Hints = new Dictionary<string, object> { { "NotARealHint", "value" } },
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
            var entry = new StubConfigurationEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "Volume",
                SettingType = typeof(int),
                DefaultValue = 50,
                AcceptableValueRange = new CommonAbstractions.AcceptableValueRange(0, 100),
            };

            provider.LoadPersistedValues([entry]);

            var bound = file.Bind("General", "Volume", 0);
            bound.Description.AcceptableValues.Should().BeOfType<AcceptableValueRange<int>>();
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

            var entry1 = new StubConfigurationEntry
            {
                ModId = "mod1",
                Section = "General",
                Key = "A",
                SettingType = typeof(int),
                DefaultValue = 1,
            };
            var entry2 = new StubConfigurationEntry
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
            var entry = new StubConfigurationEntry
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

        private sealed class StubConfigurationEntry : CommonInterfaces.IConfigurationEntry
        {
            public int SetValueCallCount { get; private set; }
            public object? LastSetValue { get; private set; }

            public string ModId { get; set; } = "mod";
            public string ModName { get; set; } = "Mod";
            public string ModVersion { get; set; } = "1.0";
            public string Section { get; set; } = "Section";
            public string Key { get; set; } = "Key";
            public string DisplayName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public Type SettingType { get; set; } = typeof(int);
            public object DefaultValue { get; set; } = 0;
            public object[] AcceptableValues { get; set; } = [];
            public CommonAbstractions.AcceptableValueRange? AcceptableValueRange { get; set; }
            public bool IsAdvanced { get; set; }
            public int Order { get; set; }
            public IDictionary<string, object>? Hints { get; set; }

            public object GetValue()
            {
                return DefaultValue;
            }

            public void SetValue(object value)
            {
                SetValueCallCount++;
                LastSetValue = value;
            }
        }
    }
}
