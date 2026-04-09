// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System;
using System.Globalization;
using AwesomeAssertions;
using ConfigurationManager.Config;
using ConfigurationManager.Input;
using UnityEngine;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class ConfigConverterTests
    {
        [Fact]
        public void RoundTrip_String_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString("hello", typeof(string));
            ConfigConverter.ConvertToObject(result, typeof(string)).Should().Be("hello");
        }

        [Fact]
        public void RoundTrip_Bool_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString(true, typeof(bool));
            ConfigConverter.ConvertToObject(result, typeof(bool)).Should().Be(true);
        }

        [Fact]
        public void RoundTrip_Int_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString(42, typeof(int));
            ConfigConverter.ConvertToObject(result, typeof(int)).Should().Be(42);
        }

        [Fact]
        public void RoundTrip_Long_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString(123456789L, typeof(long));
            ConfigConverter.ConvertToObject(result, typeof(long)).Should().Be(123456789L);
        }

        [Fact]
        public void RoundTrip_Float_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString(3.14f, typeof(float));
            ConfigConverter.ConvertToObject(result, typeof(float)).Should().Be(3.14f);
        }

        [Fact]
        public void RoundTrip_Double_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString(2.718, typeof(double));
            ConfigConverter.ConvertToObject(result, typeof(double)).Should().Be(2.718);
        }

        [Fact]
        public void RoundTrip_Decimal_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString(99.99m, typeof(decimal));
            ConfigConverter.ConvertToObject(result, typeof(decimal)).Should().Be(99.99m);
        }

        [Fact]
        public void RoundTrip_Short_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString((short)7, typeof(short));
            ConfigConverter.ConvertToObject(result, typeof(short)).Should().Be((short)7);
        }

        [Fact]
        public void RoundTrip_Byte_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString((byte)255, typeof(byte));
            ConfigConverter.ConvertToObject(result, typeof(byte)).Should().Be((byte)255);
        }

        [Fact]
        public void RoundTrip_KeyCode_ShouldPreserveValue()
        {
            var result = ConfigConverter.ConvertToString(KeyCode.F1, typeof(KeyCode));
            ConfigConverter.ConvertToObject(result, typeof(KeyCode)).Should().Be(KeyCode.F1);
        }

        [Fact]
        public void RoundTrip_Color_ShouldPreserveValue()
        {
            var color = new Color(0.5f, 0.25f, 0.75f, 1f);
            var result = ConfigConverter.ConvertToString(color, typeof(Color));
            var parsed = (Color)ConfigConverter.ConvertToObject(result, typeof(Color));

            parsed.r.Should().Be(0.5f);
            parsed.g.Should().Be(0.25f);
            parsed.b.Should().Be(0.75f);
            parsed.a.Should().Be(1f);
        }

        [Fact]
        public void RoundTrip_Vector2_ShouldPreserveValue()
        {
            var vec = new Vector2(1.5f, 2.5f);
            var result = ConfigConverter.ConvertToString(vec, typeof(Vector2));
            var parsed = (Vector2)ConfigConverter.ConvertToObject(result, typeof(Vector2));

            parsed.x.Should().Be(1.5f);
            parsed.y.Should().Be(2.5f);
        }

        [Fact]
        public void RoundTrip_Vector3_ShouldPreserveValue()
        {
            var vec = new Vector3(1f, 2f, 3f);
            var result = ConfigConverter.ConvertToString(vec, typeof(Vector3));
            var parsed = (Vector3)ConfigConverter.ConvertToObject(result, typeof(Vector3));

            parsed.x.Should().Be(1f);
            parsed.y.Should().Be(2f);
            parsed.z.Should().Be(3f);
        }

        [Fact]
        public void RoundTrip_Vector4_ShouldPreserveValue()
        {
            var vec = new Vector4(1f, 2f, 3f, 4f);
            var result = ConfigConverter.ConvertToString(vec, typeof(Vector4));
            var parsed = (Vector4)ConfigConverter.ConvertToObject(result, typeof(Vector4));

            parsed.x.Should().Be(1f);
            parsed.y.Should().Be(2f);
            parsed.z.Should().Be(3f);
            parsed.w.Should().Be(4f);
        }

        [Fact]
        public void RoundTrip_Quaternion_ShouldPreserveValue()
        {
            var quat = new Quaternion(0.1f, 0.2f, 0.3f, 0.4f);
            var result = ConfigConverter.ConvertToString(quat, typeof(Quaternion));
            var parsed = (Quaternion)ConfigConverter.ConvertToObject(result, typeof(Quaternion));

            parsed.x.Should().Be(0.1f);
            parsed.y.Should().Be(0.2f);
            parsed.z.Should().Be(0.3f);
            parsed.w.Should().Be(0.4f);
        }

        [Fact]
        public void RoundTrip_KeyboardShortcut_ShouldPreserveValue()
        {
            var shortcut = new KeyboardShortcut(KeyCode.A, KeyCode.LeftControl);
            var result = ConfigConverter.ConvertToString(shortcut, typeof(KeyboardShortcut));
            var parsed = (KeyboardShortcut)
                ConfigConverter.ConvertToObject(result, typeof(KeyboardShortcut));

            parsed.MainKey.Should().Be(KeyCode.A);
        }

        [Fact]
        public void GetConverter_UnknownNonEnumType_ShouldReturnNull()
        {
            var converter = ConfigConverter.GetConverter(typeof(Uri));

            converter.Should().BeNull();
        }

        [Fact]
        public void GetConverter_NullType_ShouldThrowArgumentNullException()
        {
            Action act = () => ConfigConverter.GetConverter(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetConverter_EnumType_ShouldHandleGenerically()
        {
            var converter = ConfigConverter.GetConverter(typeof(StringComparison));

            converter.Should().NotBeNull();
            var result = converter.ConvertToString(
                StringComparison.OrdinalIgnoreCase,
                typeof(StringComparison)
            );
            result.Should().Be("OrdinalIgnoreCase");
            converter
                .ConvertToObject("OrdinalIgnoreCase", typeof(StringComparison))
                .Should()
                .Be(StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ConvertToString_UnknownType_ShouldFallBackToToString()
        {
            var uri = new Uri("https://example.com");

            var result = ConfigConverter.ConvertToString(uri, typeof(Uri));

            result.Should().Be("https://example.com/");
        }

        [Fact]
        public void ConvertToString_NullValue_ShouldReturnEmptyString()
        {
            var result = ConfigConverter.ConvertToString(null, typeof(Uri));

            result.Should().BeEmpty();
        }

        [Fact]
        public void ConvertToObject_UnknownType_ShouldFallBackToConvertChangeType()
        {
            // Convert.ChangeType can handle string -> int via IConvertible
            // Use a type not registered: we need something that ChangeType supports
            // but isn't in the registered converters.
            // Actually int IS registered. Let's test with a non-registered IConvertible type.
            // uint is not registered.
            var result = ConfigConverter.ConvertToObject("42", typeof(uint));

            result.Should().Be(42u);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("1 2")]
        public void ConvertToObject_Color_TooFewComponents_ShouldThrowFormatException(string input)
        {
            Action act = () => ConfigConverter.ConvertToObject(input, typeof(Color));

            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void ConvertToObject_Color_ThreeComponents_ShouldDefaultAlphaToOne()
        {
            var parsed = (Color)ConfigConverter.ConvertToObject("0.5 0.25 0.75", typeof(Color));

            parsed.r.Should().Be(0.5f);
            parsed.g.Should().Be(0.25f);
            parsed.b.Should().Be(0.75f);
            parsed.a.Should().Be(1f);
        }

        [Fact]
        public void ConvertToObject_Color_NonNumericValue_ShouldThrowFormatException()
        {
            Action act = () => ConfigConverter.ConvertToObject("abc 0.5 0.5", typeof(Color));

            act.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        public void ConvertToObject_Vector2_TooFewComponents_ShouldThrowFormatException(
            string input
        )
        {
            Action act = () => ConfigConverter.ConvertToObject(input, typeof(Vector2));

            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void ConvertToObject_Vector2_NonNumericValue_ShouldThrowFormatException()
        {
            Action act = () => ConfigConverter.ConvertToObject("abc 1", typeof(Vector2));

            act.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("1 2")]
        public void ConvertToObject_Vector3_TooFewComponents_ShouldThrowFormatException(
            string input
        )
        {
            Action act = () => ConfigConverter.ConvertToObject(input, typeof(Vector3));

            act.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("1 2")]
        [InlineData("1 2 3")]
        public void ConvertToObject_Vector4_TooFewComponents_ShouldThrowFormatException(
            string input
        )
        {
            Action act = () => ConfigConverter.ConvertToObject(input, typeof(Vector4));

            act.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("1 2")]
        [InlineData("1 2 3")]
        public void ConvertToObject_Quaternion_TooFewComponents_ShouldThrowFormatException(
            string input
        )
        {
            Action act = () => ConfigConverter.ConvertToObject(input, typeof(Quaternion));

            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void AddConverter_ShouldOverwriteExistingConverter()
        {
            var originalConverter = ConfigConverter.GetConverter(typeof(int));
            var customConverter = new TypeConverter
            {
                ConvertToString = (o, _) => "custom:" + o,
                ConvertToObject = (s, _) =>
                    int.Parse(
                        s.Replace("custom:", "", StringComparison.Ordinal),
                        CultureInfo.InvariantCulture
                    ),
            };

            try
            {
                ConfigConverter.AddConverter(typeof(int), customConverter);
                ConfigConverter.ConvertToString(5, typeof(int)).Should().Be("custom:5");
            }
            finally
            {
                // Restore original to not affect other tests
                ConfigConverter.AddConverter(typeof(int), originalConverter);
            }
        }
    }
}
