// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using ConfigurationManager.Config;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class AcceptableValueRangeTests
    {
        [Fact]
        public void Constructor_MaxLessThanMin_ShouldThrowArgumentException()
        {
            Action act = () => _ = new AcceptableValueRange<int>(10, 5);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Clamp_ValueBelowMin_ShouldReturnMin()
        {
            var range = new AcceptableValueRange<int>(0, 100);

            range.Clamp(-5).Should().Be(0);
        }

        [Fact]
        public void Clamp_ValueAboveMax_ShouldReturnMax()
        {
            var range = new AcceptableValueRange<int>(0, 100);

            range.Clamp(150).Should().Be(100);
        }

        [Fact]
        public void Clamp_ValueInRange_ShouldReturnValue()
        {
            var range = new AcceptableValueRange<int>(0, 100);

            range.Clamp(50).Should().Be(50);
        }

        [Fact]
        public void ValueType_ShouldReturnTypeOfT()
        {
            var range = new AcceptableValueRange<float>(0f, 1f);

            range.ValueType.Should().Be<float>();
        }
    }
}
