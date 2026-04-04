// SPDX-License-Identifier: MIT

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
    }
}
