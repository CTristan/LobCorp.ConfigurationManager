// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System;
using AwesomeAssertions;
using ConfigurationManager.Utilities;
using UnityEngine;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class UtilsTests
    {
        [Fact]
        public void ToProperCase_Empty_ShouldReturnEmpty()
        {
            "".ToProperCase().Should().BeEmpty();
        }

        [Fact]
        public void ToProperCase_Null_ShouldReturnEmpty()
        {
            ((string)null!).ToProperCase().Should().BeEmpty();
        }

        [Fact]
        public void ToProperCase_SingleChar_ShouldReturnUnchanged()
        {
            "a".ToProperCase().Should().Be("a");
        }

        [Fact]
        public void ToProperCase_CamelCase_ShouldInsertSpaces()
        {
            "camelCase".ToProperCase().Should().Be("Camel Case");
        }

        [Fact]
        public void ToProperCase_AlreadyCapitalized_ShouldInsertSpaces()
        {
            "MyProperty".ToProperCase().Should().Be("My Property");
        }

        [Fact]
        public void AppendZero_NoDecimal_ShouldAddDotZero()
        {
            "42".AppendZero().Should().Be("42.0");
        }

        [Fact]
        public void AppendZero_HasDecimal_ShouldReturnUnchanged()
        {
            "42.5".AppendZero().Should().Be("42.5");
        }

        [Fact]
        public void AppendZero_Null_ShouldThrowArgumentNullException()
        {
            Action act = () => ((string)null!).AppendZero();

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AppendZeroIfFloat_FloatType_ShouldAppend()
        {
            "42".AppendZeroIfFloat(typeof(float)).Should().Be("42.0");
        }

        [Fact]
        public void AppendZeroIfFloat_DoubleType_ShouldAppend()
        {
            "42".AppendZeroIfFloat(typeof(double)).Should().Be("42.0");
        }

        [Fact]
        public void AppendZeroIfFloat_DecimalType_ShouldAppend()
        {
            "42".AppendZeroIfFloat(typeof(decimal)).Should().Be("42.0");
        }

        [Fact]
        public void AppendZeroIfFloat_IntType_ShouldReturnUnchanged()
        {
            "42".AppendZeroIfFloat(typeof(int)).Should().Be("42");
        }

        [Fact]
        public void FillTexture_NullTexture_ShouldThrowArgumentNullException()
        {
            Action act = () => ((Texture2D)null!).FillTexture(Color.white);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FillTextureCheckerboard_NullTexture_ShouldThrowArgumentNullException()
        {
            Action act = () => ((Texture2D)null!).FillTextureCheckerboard();

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
