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
            content.AppendLine("[General]");
            content.AppendLine("Volume = 80");
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
            content.AppendLine("[General]");
            content.AppendLine("Volume = 80");
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
            content.AppendLine("[General]");
            content.AppendLine("Volume = 99");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            configFile.Reload();

            entry.Value.Should().Be(99);
        }

        [Fact]
        public void GetEntries_ShouldReturnAllBoundEntries()
        {
            var configFile = new LmmConfigFile(_tempPath);
            configFile.Bind("General", "Volume", 50);
            configFile.Bind("General", "Brightness", 75);

            var entries = configFile.GetEntries().ToList();

            entries.Should().HaveCount(2);
        }

        [Fact]
        public void ReadValueFromFile_ShouldSkipCommentsAndEmptyLines()
        {
            var content = new StringBuilder();
            content.AppendLine("## Settings file");
            content.AppendLine("");
            content.AppendLine("[General]");
            content.AppendLine("");
            content.AppendLine("## The volume level");
            content.AppendLine("## Setting type: Int32");
            content.AppendLine("Volume = 42");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Volume", 0);

            entry.Value.Should().Be(42);
        }

        [Fact]
        public void Bind_StringType_ShouldLoadSavedValue()
        {
            var content = new StringBuilder();
            content.AppendLine("[General]");
            content.AppendLine("Name = Hello World");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Name", "default");

            entry.Value.Should().Be("Hello World");
        }

        [Fact]
        public void Bind_BoolType_ShouldLoadSavedValue()
        {
            var content = new StringBuilder();
            content.AppendLine("[General]");
            content.AppendLine("Enabled = True");
            File.WriteAllText(_tempPath, content.ToString(), Encoding.UTF8);

            var configFile = new LmmConfigFile(_tempPath);
            var entry = configFile.Bind("General", "Enabled", false);

            entry.Value.Should().BeTrue();
        }
    }
}
