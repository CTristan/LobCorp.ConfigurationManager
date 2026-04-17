// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration.Tests
{
    public sealed class BindingModelTests
    {
        [Fact]
        public void Equals_IdenticalInstances_ReturnsTrue()
        {
            var a = Create();
            var b = Create();
            a.Equals(b).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void Equals_NullOther_ReturnsFalse()
        {
            Create().Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Equals_ObjectOverload_ReturnsFalseForUnrelatedType()
        {
            Create().Equals((object?)"not a model").Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentContainingType_ReturnsFalse()
        {
            Create(containingType: "Other.Type").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentSection_ReturnsFalse()
        {
            Create(section: "OtherSection").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentKey_ReturnsFalse()
        {
            Create(key: "OtherKey").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentValueType_ReturnsFalse()
        {
            Create(valueType: "System.Single").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentDefaultLiteral_ReturnsFalse()
        {
            Create(defaultLiteral: "1").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentNamedArgumentCount_ReturnsFalse()
        {
            var a = Create(namedArgs: new Dictionary<string, string> { ["order"] = "1" });
            var b = Create();
            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentNamedArgumentValue_ReturnsFalse()
        {
            var a = Create(namedArgs: new Dictionary<string, string> { ["order"] = "1" });
            var b = Create(namedArgs: new Dictionary<string, string> { ["order"] = "2" });
            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void Equals_MissingNamedArgumentKey_ReturnsFalse()
        {
            var a = Create(namedArgs: new Dictionary<string, string> { ["order"] = "1" });
            var b = Create(namedArgs: new Dictionary<string, string> { ["otherKey"] = "1" });
            a.Equals(b).Should().BeFalse();
        }

        private static BindingModel Create(
            string containingType = "TestMod.MyConfig",
            string section = "Section",
            string key = "Key",
            string valueType = "System.Int32",
            string defaultLiteral = "100",
            Dictionary<string, string>? namedArgs = null
        )
        {
            return new BindingModel(
                containingType,
                section,
                key,
                valueType,
                defaultLiteral,
                namedArgs ?? [],
                Location.None
            );
        }
    }

    public sealed class ModAttributeModelTests
    {
        [Fact]
        public void Equals_IdenticalInstances_ReturnsTrue()
        {
            var a = Create();
            var b = Create();
            a.Equals(b).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void Equals_NullOther_ReturnsFalse()
        {
            Create().Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Equals_ObjectOverload_ReturnsFalseForUnrelatedType()
        {
            Create().Equals((object?)42).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentModId_ReturnsFalse()
        {
            Create(modId: "other.id").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentModName_ReturnsFalse()
        {
            Create(modName: "Other").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentModVersion_ReturnsFalse()
        {
            Create(modVersion: "2.0.0").Equals(Create()).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentFallback_ReturnsFalse()
        {
            Create(fallback: "FileBacked").Equals(Create()).Should().BeFalse();
        }

        private static ModAttributeModel Create(
            string modId = "com.test.mod",
            string modName = "Test Mod",
            string modVersion = "1.0.0",
            string fallback = "InMemory"
        )
        {
            return new ModAttributeModel(modId, modName, modVersion, fallback, Location.None);
        }
    }
}
