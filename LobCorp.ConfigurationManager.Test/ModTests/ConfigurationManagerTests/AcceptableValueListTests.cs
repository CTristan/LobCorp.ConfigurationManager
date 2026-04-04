// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using ConfigurationManager.Config;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class AcceptableValueListTests
    {
        [Fact]
        public void Constructor_NullArray_ShouldThrowArgumentException()
        {
            Action act = () => _ = new AcceptableValueList<string>(null);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_EmptyArray_ShouldThrowArgumentException()
        {
            Action act = () => _ = new AcceptableValueList<string>();

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void IsValid_ValueInList_ShouldReturnTrue()
        {
            var list = new AcceptableValueList<string>("a", "b", "c");

            list.IsValid("b").Should().BeTrue();
        }

        [Fact]
        public void IsValid_ValueNotInList_ShouldReturnFalse()
        {
            var list = new AcceptableValueList<string>("a", "b", "c");

            list.IsValid("d").Should().BeFalse();
        }

        [Fact]
        public void ValueType_ShouldReturnTypeOfT()
        {
            var list = new AcceptableValueList<int>(1, 2, 3);

            list.ValueType.Should().Be<int>();
        }
    }
}
