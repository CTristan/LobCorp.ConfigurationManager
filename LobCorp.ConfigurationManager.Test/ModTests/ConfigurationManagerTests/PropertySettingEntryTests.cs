// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System;
using System.ComponentModel;
using AwesomeAssertions;
using ConfigurationManager.Implementations;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class PropertySettingEntryTests
    {
        private sealed class TestObject
        {
            public string ReadWriteProperty { get; set; } = string.Empty;

            public string ReadOnlyProperty { get; }

            [DisplayName("Custom Name")]
            [Description("A described property")]
            public int NamedProperty { get; set; }

            [Browsable(false)]
            public string HiddenProperty { get; set; } = string.Empty;

            public TestObject()
            {
                ReadOnlyProperty = "readonly";
            }
        }

        [Fact]
        public void GetValue_ShouldDelegateToPropertyInfoGetValue()
        {
            var obj = new TestObject { ReadWriteProperty = "hello" };
            var prop = typeof(TestObject).GetProperty("ReadWriteProperty");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.GetValue().Should().Be("hello");
        }

        [Fact]
        public void DispName_ShouldFallBackToPropertyName_WhenNotSetViaAttributes()
        {
            var obj = new TestObject();
            var prop = typeof(TestObject).GetProperty("ReadWriteProperty");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DispName.Should().Be("ReadWriteProperty");
        }

        [Fact]
        public void DispName_ShouldUseDisplayNameAttribute_WhenSet()
        {
            var obj = new TestObject();
            var prop = typeof(TestObject).GetProperty("NamedProperty");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DispName.Should().Be("Custom Name");
        }

        [Fact]
        public void Browsable_ShouldDefaultToTrue_WhenCanReadAndCanWrite()
        {
            var obj = new TestObject();
            var prop = typeof(TestObject).GetProperty("ReadWriteProperty");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Browsable.Should().BeTrue();
        }

        [Fact]
        public void Browsable_ShouldDefaultToFalse_WhenReadOnly()
        {
            var obj = new TestObject();
            var prop = typeof(TestObject).GetProperty("ReadOnlyProperty");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Browsable.Should().BeFalse();
        }

        [Fact]
        public void Browsable_ShouldRespectBrowsableAttribute()
        {
            var obj = new TestObject();
            var prop = typeof(TestObject).GetProperty("HiddenProperty");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Browsable.Should().BeFalse();
        }

        [Fact]
        public void SettingType_ShouldReturnPropertyType()
        {
            var obj = new TestObject();
            var prop = typeof(TestObject).GetProperty("NamedProperty");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.SettingType.Should().Be<int>();
        }

        [Fact]
        public void ReadOnly_ShouldBeSetToCanWrite()
        {
            var obj = new TestObject();
            var writableProp = typeof(TestObject).GetProperty("ReadWriteProperty");
            var readOnlyProp = typeof(TestObject).GetProperty("ReadOnlyProperty");

            var writableEntry = new PropertySettingEntry(obj, writableProp, null);
            var readOnlyEntry = new PropertySettingEntry(obj, readOnlyProp, null);

            // PropertySettingEntry sets ReadOnly = settingProp.CanWrite
            writableEntry.ReadOnly.Should().BeTrue();
            readOnlyEntry.ReadOnly.Should().BeFalse();
        }

        [Fact]
        public void Constructor_NullProperty_ShouldThrowArgumentNullException()
        {
            var obj = new TestObject();
            Action act = () => _ = new PropertySettingEntry(obj, null!, null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
