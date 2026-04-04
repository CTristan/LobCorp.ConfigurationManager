// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using ConfigurationManager;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class PluginInfoTests
    {
        [Fact]
        public void Equals_SameGuid_ShouldReturnTrue()
        {
            var a = new PluginInfo("guid1", "Name1", "1.0");
            var b = new PluginInfo("guid1", "Name2", "2.0");

            a.Equals(b).Should().BeTrue();
        }

        [Fact]
        public void Equals_DifferentGuid_ShouldReturnFalse()
        {
            var a = new PluginInfo("guid1", "Name", "1.0");
            var b = new PluginInfo("guid2", "Name", "1.0");

            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_SameGuid_ShouldProduceSameHash()
        {
            var a = new PluginInfo("guid1", "Name1", "1.0");
            var b = new PluginInfo("guid1", "Name2", "2.0");

            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void ToString_ShouldReturnNameSpaceVersion()
        {
            var info = new PluginInfo("guid", "MyMod", "1.2.3");

            info.ToString().Should().Be("MyMod 1.2.3");
        }

        [Fact]
        public void Constructor_NullParameters_ShouldDefaultToEmptyString()
        {
            var info = new PluginInfo(null, null, null);

            info.GUID.Should().BeEmpty();
            info.Name.Should().BeEmpty();
            info.Version.Should().BeEmpty();
        }
    }
}
