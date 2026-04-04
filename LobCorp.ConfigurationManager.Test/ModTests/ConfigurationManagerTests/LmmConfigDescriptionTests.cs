// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using AwesomeAssertions;
using ConfigurationManager.Config;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class LmmConfigDescriptionTests
    {
        [Fact]
        public void Constructor_ShouldSetDescriptionAcceptableValuesAndTags()
        {
            var range = new AcceptableValueRange<int>(0, 100);
            var tag = new object();

            var desc = new LmmConfigDescription("A description", range, tag);

            desc.Description.Should().Be("A description");
            desc.AcceptableValues.Should().BeSameAs(range);
            desc.Tags.Should().HaveCount(1);
            desc.Tags[0].Should().BeSameAs(tag);
        }

        [Fact]
        public void Constructor_NullTags_ShouldDefaultToEmptyArray()
        {
            var desc = new LmmConfigDescription("desc", null, null);

            desc.Tags.Should().BeEmpty();
        }
    }
}
