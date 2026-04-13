// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System;
using System.IO;
using AwesomeAssertions;
using ConfigurationManager.Config;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class LmmConfigEntryTests : IDisposable
    {
        private readonly string _tempPath;
        private readonly LmmConfigFile _configFile;

        public LmmConfigEntryTests()
        {
            _tempPath = Path.GetTempFileName();
            _configFile = new LmmConfigFile(_tempPath);
        }

        public void Dispose()
        {
            if (File.Exists(_tempPath))
            {
                File.Delete(_tempPath);
            }
        }

        [Fact]
        public void Value_ShouldReturnDefaultInitially()
        {
            var entry = _configFile.Bind("General", "Volume", 50);

            entry.Value.Should().Be(50);
        }

        [Fact]
        public void Value_Setter_ShouldTriggerSettingChangedEvent()
        {
            var entry = _configFile.Bind("General", "Volume", 50);
            var eventFired = false;
            entry.SettingChanged += (sender, args) => eventFired = true;

            entry.Value = 75;

            eventFired.Should().BeTrue();
        }

        [Fact]
        public void Value_Setter_SameValue_ShouldNotFireEvent()
        {
            var entry = _configFile.Bind("General", "Volume", 50);
            var eventFired = false;
            entry.SettingChanged += (sender, args) => eventFired = true;

            entry.Value = 50;

            eventFired.Should().BeFalse();
        }

        [Fact]
        public void Value_Setter_ShouldCallConfigFileSave()
        {
            var entry = _configFile.Bind("General", "Volume", 50);

            entry.Value = 75;

            // Verify save occurred by checking file contents
            var text = File.ReadAllText(_tempPath);
            text.Should().Contain("Volume = 75");
        }

        [Fact]
        public void BoxedValue_Getter_ShouldDelegateToValue()
        {
            var entry = _configFile.Bind("General", "Volume", 50);

            entry.BoxedValue.Should().Be(50);
        }

        [Fact]
        public void BoxedValue_Setter_ShouldDelegateToValue()
        {
            var entry = _configFile.Bind("General", "Volume", 50);

            entry.BoxedValue = 75;

            entry.Value.Should().Be(75);
        }

        [Fact]
        public void SettingType_ShouldReturnTypeOfT()
        {
            var entry = _configFile.Bind("General", "Volume", 50);

            entry.SettingType.Should().Be<int>();
        }

        [Fact]
        public void Value_WithAcceptableValueRange_ShouldClampToMaximum()
        {
            var range = new AcceptableValueRange<int>(0, 100);
            var desc = new LmmConfigDescription("Volume", range);
            var entry = _configFile.Bind("General", "Volume", 50, desc);

            entry.Value = 150;

            entry.Value.Should().Be(100);
        }

        [Fact]
        public void Value_WithAcceptableValueRange_ShouldClampToMinimum()
        {
            var range = new AcceptableValueRange<int>(0, 100);
            var desc = new LmmConfigDescription("Volume", range);
            var entry = _configFile.Bind("General", "Volume", 50, desc);

            entry.Value = -10;

            entry.Value.Should().Be(0);
        }

        [Fact]
        public void Value_WithAcceptableValueList_ShouldRejectInvalidValue()
        {
            var list = new AcceptableValueList<string>("Low", "Medium", "High");
            var desc = new LmmConfigDescription("Quality", list);
            var entry = _configFile.Bind("General", "Quality", "Medium", desc);

            entry.Value = "Ultra";

            entry.Value.Should().Be("Medium");
        }

        [Fact]
        public void Value_WithAcceptableValueList_ShouldAcceptValidValue()
        {
            var list = new AcceptableValueList<string>("Low", "Medium", "High");
            var desc = new LmmConfigDescription("Quality", list);
            var entry = _configFile.Bind("General", "Quality", "Medium", desc);

            entry.Value = "High";

            entry.Value.Should().Be("High");
        }

        [Fact]
        public void BeginBatchSave_ShouldSuppressSavesDuringScope()
        {
            var entry = _configFile.Bind("General", "Volume", 50);

            using (_configFile.BeginBatchSave())
            {
                entry.Value = 75;

                var text = File.ReadAllText(_tempPath);
                text.Should().NotContain("Volume = 75");
            }

            var finalText = File.ReadAllText(_tempPath);
            finalText.Should().Contain("Volume = 75");
        }

        [Fact]
        public void BeginBatchSave_ShouldSaveAllValuesOnDispose()
        {
            var entry1 = _configFile.Bind("General", "Volume", 50);
            var entry2 = _configFile.Bind("General", "Brightness", 100);

            using (_configFile.BeginBatchSave())
            {
                entry1.Value = 75;
                entry2.Value = 200;
            }

            var text = File.ReadAllText(_tempPath);
            text.Should().Contain("Volume = 75");
            text.Should().Contain("Brightness = 200");
        }
    }
}
