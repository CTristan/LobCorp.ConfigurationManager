// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using AwesomeAssertions;
using ConfigurationManager;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class ValueChangedEventArgsTests
    {
        [Fact]
        public void Constructor_ShouldSetNewValue()
        {
            var args = new ValueChangedEventArgs<int>(42);

            args.NewValue.Should().Be(42);
        }

        [Fact]
        public void Constructor_StringValue_ShouldSetNewValue()
        {
            var args = new ValueChangedEventArgs<string>("hello");

            args.NewValue.Should().Be("hello");
        }
    }
}
