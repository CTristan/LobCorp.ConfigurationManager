// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System;
using System.IO;
using AwesomeAssertions;
using ConfigurationManager.Config;
using ConfigurationManager.Implementations;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class LmmSettingEntryTests
    {
        private readonly string _tempPath;
        private readonly LmmConfigFile _configFile;

        public LmmSettingEntryTests()
        {
            _tempPath = Path.GetTempFileName();
            _configFile = new LmmConfigFile(_tempPath);
        }

        [Fact]
        public void Constructor_ShouldPopulateDispNameFromKey()
        {
            var entry = _configFile.Bind("General", "Volume", 50, "The volume");
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.DispName.Should().Be("Volume");
        }

        [Fact]
        public void Constructor_ShouldPopulateCategoryFromSection()
        {
            var entry = _configFile.Bind("Audio", "Volume", 50);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.Category.Should().Be("Audio");
        }

        [Fact]
        public void Constructor_ShouldPopulateDescriptionFromDescription()
        {
            var entry = _configFile.Bind("General", "Volume", 50, "The volume level");
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.Description.Should().Be("The volume level");
        }

        [Fact]
        public void Constructor_ShouldSetObjToStrConverter()
        {
            var entry = _configFile.Bind("General", "Volume", 50);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.ObjToStr.Should().NotBeNull();
            setting.ObjToStr(42).Should().Be("42");
        }

        [Fact]
        public void Constructor_ShouldSetStrToObjConverter()
        {
            var entry = _configFile.Bind("General", "Volume", 50);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.StrToObj.Should().NotBeNull();
            setting.StrToObj("42").Should().Be(42);
        }

        [Fact]
        public void Constructor_AcceptableValueList_ShouldPopulateAcceptableValues()
        {
            var valueList = new AcceptableValueList<string>("Low", "Medium", "High");
            var desc = new LmmConfigDescription("Quality setting", valueList);
            var entry = _configFile.Bind("General", "Quality", "Medium", desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.AcceptableValues.Should().NotBeNull();
            setting.AcceptableValues.Should().HaveCount(3);
            setting.AcceptableValues.Should().Contain("Low");
            setting.AcceptableValues.Should().Contain("Medium");
            setting.AcceptableValues.Should().Contain("High");
        }

        [Fact]
        public void Constructor_AcceptableValueRange_ShouldPopulateAcceptableValueRange()
        {
            var valueRange = new AcceptableValueRange<int>(0, 100);
            var desc = new LmmConfigDescription("Volume level", valueRange);
            var entry = _configFile.Bind("General", "Volume", 50, desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.AcceptableValueRange.Key.Should().Be(0);
            setting.AcceptableValueRange.Value.Should().Be(100);
        }

        [Fact]
        public void ShowRangeAsPercent_IntRange0To100_ShouldBeTrue()
        {
            var valueRange = new AcceptableValueRange<int>(0, 100);
            var desc = new LmmConfigDescription("Percent", valueRange);
            var entry = _configFile.Bind("General", "Percent", 50, desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.ShowRangeAsPercent.Should().BeTrue();
        }

        [Fact]
        public void ShowRangeAsPercent_FloatRange0To1_ShouldBeTrue()
        {
            var valueRange = new AcceptableValueRange<float>(0f, 1f);
            var desc = new LmmConfigDescription("Normalized", valueRange);
            var entry = _configFile.Bind("General", "Alpha", 0.5f, desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.ShowRangeAsPercent.Should().BeTrue();
        }

        [Fact]
        public void Get_ShouldDelegateToEntryBoxedValue()
        {
            var entry = _configFile.Bind("General", "Volume", 50);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");
            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.Get().Should().Be(50);
        }

        [Fact]
        public void Set_ShouldDelegateToEntryBoxedValue()
        {
            var entry = _configFile.Bind("General", "Volume", 50);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");
            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.Set(75);

            entry.Value.Should().Be(75);
        }

        [Fact]
        public void SettingType_ShouldDelegateToEntrySettingType()
        {
            var entry = _configFile.Bind("General", "Volume", 50);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");
            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.SettingType.Should().Be<int>();
        }

        [Fact]
        public void Constructor_StringTag_ReadOnly_ShouldSetReadOnly()
        {
            var desc = new LmmConfigDescription("test", null, "ReadOnly");
            var entry = _configFile.Bind("General", "ReadOnlySetting", 50, desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.ReadOnly.Should().BeTrue();
        }

        [Fact]
        public void Constructor_StringTag_Advanced_ShouldSetIsAdvanced()
        {
            var desc = new LmmConfigDescription("test", null, "Advanced");
            var entry = _configFile.Bind("General", "AdvancedSetting", 50, desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.IsAdvanced.Should().BeTrue();
        }

        [Fact]
        public void Constructor_StringTag_Hidden_ShouldSetBrowsableFalse()
        {
            var desc = new LmmConfigDescription("test", null, "Hidden");
            var entry = _configFile.Bind("General", "HiddenSetting", 50, desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.Browsable.Should().BeFalse();
        }

        [Fact]
        public void Constructor_NullDescription_ShouldHandleGracefully()
        {
            var desc = new LmmConfigDescription(null);
            var entry = _configFile.Bind("General", "NullDescSetting", 50, desc);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.Description.Should().BeNull();
        }

        [Fact]
        public void Constructor_DefaultValue_ShouldBeSetFromEntry()
        {
            var entry = _configFile.Bind("General", "DefaultSetting", 42);
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");

            var setting = new LmmSettingEntry(entry, pluginInfo, null);

            setting.DefaultValue.Should().Be(42);
        }

        [Fact]
        public void Constructor_NullEntry_ShouldThrowArgumentNullException()
        {
            var pluginInfo = new PluginInfo("test", "TestMod", "1.0");
            Action act = () => _ = new LmmSettingEntry(null!, pluginInfo, null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
