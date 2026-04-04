// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using AwesomeAssertions;
using ConfigurationManager.Config;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class LmmConfigDefinitionTests
    {
        [Fact]
        public void Equals_SameSectionAndKey_ShouldReturnTrue()
        {
            var a = new LmmConfigDefinition("Section", "Key");
            var b = new LmmConfigDefinition("Section", "Key");

            a.Equals(b).Should().BeTrue();
        }

        [Fact]
        public void Equals_DifferentSection_ShouldReturnFalse()
        {
            var a = new LmmConfigDefinition("Section1", "Key");
            var b = new LmmConfigDefinition("Section2", "Key");

            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentKey_ShouldReturnFalse()
        {
            var a = new LmmConfigDefinition("Section", "Key1");
            var b = new LmmConfigDefinition("Section", "Key2");

            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void Equals_Null_ShouldReturnFalse()
        {
            var a = new LmmConfigDefinition("Section", "Key");

            a.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_EqualDefinitions_ShouldProduceEqualHashes()
        {
            var a = new LmmConfigDefinition("Section", "Key");
            var b = new LmmConfigDefinition("Section", "Key");

            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void ToString_ShouldReturnSectionDotKey()
        {
            var def = new LmmConfigDefinition("General", "Volume");

            def.ToString().Should().Be("General.Volume");
        }

        [Fact]
        public void Constructor_NullSection_ShouldDefaultToEmptyString()
        {
            var def = new LmmConfigDefinition(null, "Key");

            def.Section.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_NullKey_ShouldDefaultToEmptyString()
        {
            var def = new LmmConfigDefinition("Section", null);

            def.Key.Should().BeEmpty();
        }
    }
}
