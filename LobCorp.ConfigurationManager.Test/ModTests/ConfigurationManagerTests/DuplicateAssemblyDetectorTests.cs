// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using AwesomeAssertions;
using ConfigurationManager.Implementations;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class DuplicateAssemblyDetectorTests
    {
        [Fact]
        public void IsDuplicateLoaded_WhenNoDuplicate_ShouldReturnFalse()
        {
            DuplicateAssemblyDetector.IsDuplicateLoaded.Should().BeFalse();
        }

        [Fact]
        public void DuplicateLocation_WhenNoDuplicate_ShouldReturnEmpty()
        {
            DuplicateAssemblyDetector.DuplicateLocation.Should().BeEmpty();
        }
    }
}
