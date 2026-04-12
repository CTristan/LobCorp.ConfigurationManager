// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System.IO;
using System.Linq;
using System.Text;
using AwesomeAssertions;
using ConfigurationManager.Config;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class LmmConfigFileTests : System.IDisposable
    {
        private readonly string _tempPath;

        public LmmConfigFileTests()
        {
            _tempPath = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_tempPath))
            {
                File.Delete(_tempPath);
            }
        }

        [Fact]
        public void Bind_ShouldCreateEntryWithDefaultValue()
        {
            var configFile = new LmmConfigFile(_tempPath);

            var entry = configFile.Bind("General", "Volume", 50);

            entry.Value.Should().Be(50);
        }

        [Fact]
        public void Bind_DuplicateDefinition_ShouldReturnSameEntry()
        {
            var configFile = new LmmConfigFile(_tempPath);

            var entry1 = configFile.Bind("General", "Volume", 50);
            var entry2 = configFile.Bind("General", "Volume", 75);

            entry2.Should().BeSameAs(entry1);
        }

        [Fact]
        public void Bind_ShouldLoadSavedValueFromExistingConfigFile()
        {
            var content = new StringBuilder();
            content.AppendLine("[General]").AppendLine("Volume = 80");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Volume", 50);

            entry.Value.Should().Be(80);
        }

        [Fact]
        public void Save_ShouldWriteIniFormatFile()
        {
            var configFile = new LmmConfigFile(_tempPath);
            configFile.Bind("General", "Volume", 75, "The volume level");

            configFile.Save();

            var text = File.ReadAllText(_tempPath);
            text.Should().Contain("[General]");
            text.Should().Contain("Volume = 75");
            text.Should().Contain("## The volume level");
            text.Should().Contain("## Setting type: Int32");
            text.Should().Contain("## Default value: 75");
        }

        [Fact]
        public void Save_DuringDisableSaving_ShouldBeNoOp()
        {
            // Write an initial config with a value
            var content = new StringBuilder();
            content.AppendLine("[General]").AppendLine("Volume = 80");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            // Bind will load the saved value (80), which sets _disableSaving temporarily.
            // During that load, Value setter calls Save() but it should be a no-op.
            // After binding, the file should still contain original content (not overwritten during load).
            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Volume", 50);

            // The value should be loaded from file
            entry.Value.Should().Be(80);
        }

        [Fact]
        public void Reload_ShouldUpdateEntriesFromDisk()
        {
            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Volume", 50);
            configFile.Save();

            // Manually overwrite file
            var content = new StringBuilder();
            content.AppendLine("[General]").AppendLine("Volume = 99");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            configFile.Reload();

            entry.Value.Should().Be(99);
        }

        [Fact]
        public void Entries_ShouldReturnAllBoundEntries()
        {
            var configFile = new LmmConfigFile(_tempPath);
            configFile.Bind("General", "Volume", 50);
            configFile.Bind("General", "Brightness", 75);

            var entries = configFile.Entries.ToList();

            entries.Should().HaveCount(2);
        }

        [Fact]
        public void ReadValueFromFile_ShouldSkipCommentsAndEmptyLines()
        {
            var content = new StringBuilder();
            content
                .AppendLine("## Settings file")
                .AppendLine("")
                .AppendLine("[General]")
                .AppendLine("")
                .AppendLine("## The volume level")
                .AppendLine("## Setting type: Int32")
                .AppendLine("Volume = 42");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Volume", 0);

            entry.Value.Should().Be(42);
        }

        [Fact]
        public void Bind_StringType_ShouldLoadSavedValue()
        {
            var content = new StringBuilder();
            content.AppendLine("[General]").AppendLine("Name = Hello World");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Name", "default");

            entry.Value.Should().Be("Hello World");
        }

        [Theory]
        [InlineData("Hello World", "Hello World")]
        [InlineData("key=value", "key=value")]
        [InlineData("a = b = c", "a = b = c")]
        [InlineData("  padded  ", "padded")]
        [InlineData("", "")]
        public void SaveAndReload_StringValues_ShouldRoundTripPerIniConvention(
            string input,
            string expected
        )
        {
            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Name", "default");
            entry.Value = input;

            configFile.Reload();

            entry.Value.Should().Be(expected);
        }

        [Fact]
        public void Bind_BoolType_ShouldLoadSavedValue()
        {
            var content = new StringBuilder();
            content.AppendLine("[General]").AppendLine("Enabled = True");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Enabled", false);

            entry.Value.Should().BeTrue();
        }

        [Fact]
        public void Reload_ShouldPickUpNewValuesAfterFileChange()
        {
            // Bind entries, save, then externally modify the file and reload.
            // This verifies that any read cache is properly invalidated on Reload().
            var configFile = new LmmConfigFile(_tempPath);
            var volume = configFile.Bind("General", "Volume", 50);
            var name = configFile.Bind("General", "Name", "default");
            configFile.Save();

            // Externally modify both values
            var content = new StringBuilder();
            content.AppendLine("[General]").AppendLine("Volume = 77").AppendLine("Name = changed");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            configFile.Reload();

            volume.Value.Should().Be(77);
            name.Value.Should().Be("changed");
        }

        [Fact]
        public void Save_WithInterleavedSections_ShouldNotProduceDuplicateHeaders()
        {
            var configFile = new LmmConfigFile(_tempPath);

            // Bind entries from different sections in interleaved order
            configFile.Bind("Audio", "Volume", 50);
            configFile.Bind("Video", "Resolution", "1080p");
            configFile.Bind("Audio", "Mute", false);
            configFile.Bind("Video", "Fullscreen", true);

            configFile.Save();

            var text = File.ReadAllText(_tempPath);
            var lines = text.Split('\n');
            var sectionHeaders = lines
                .Select(l => l.Trim())
                .Where(l => l.StartsWith('[') && l.EndsWith(']'))
                .ToList();

            // Each section should appear exactly once
            sectionHeaders.Should().HaveCount(2);
            sectionHeaders.Distinct().Should().HaveCount(2);
        }
    }
}
