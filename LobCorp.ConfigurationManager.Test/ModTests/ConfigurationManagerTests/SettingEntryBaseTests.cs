// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System.ComponentModel;
using AwesomeAssertions;
using ConfigurationManager.Implementations;
using Xunit;

#endregion

namespace LobCorp.ConfigurationManager.Test.ModTests.ConfigurationManagerTests
{
    public sealed class SettingEntryBaseTests
    {
        private sealed class PlainPropertyHolder
        {
            public string Plain { get; set; } = string.Empty;
        }

        private sealed class AttributedPropertyHolder
        {
            [DisplayName("Custom Name")]
            [Category("MyCategory")]
            [Description("A description")]
            [DefaultValue(42)]
            [ReadOnly(true)]
            [Browsable(false)]
            public int FullyAttributed { get; set; }
        }

        private sealed class NonWritableHolder(string value)
        {
            public string ReadOnlyValue { get; } = value;
        }

        [Fact]
        public void SetFromAttributes_NoAttributes_ShouldLeaveDefaultValues()
        {
            var obj = new PlainPropertyHolder();
            var prop = typeof(PlainPropertyHolder).GetProperty("Plain");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DispName.Should().Be("Plain");
        }

        [Fact]
        public void SetFromAttributes_DisplayNameAttribute_ShouldSetDispName()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DispName.Should().Be("Custom Name");
        }

        [Fact]
        public void SetFromAttributes_CategoryAttribute_ShouldSetCategory()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Category.Should().Be("MyCategory");
        }

        [Fact]
        public void SetFromAttributes_DescriptionAttribute_ShouldSetDescription()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Description.Should().Be("A description");
        }

        [Fact]
        public void SetFromAttributes_DefaultValueAttribute_ShouldSetDefaultValue()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DefaultValue.Should().Be(42);
        }

        [Fact]
        public void SetFromAttributes_ReadOnlyAttribute_ShouldSetReadOnly()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed");

            var entry = new PropertySettingEntry(obj, prop, null);

            // ReadOnlyAttribute(true) takes priority over CanWrite
            entry.ReadOnly.Should().BeTrue();
        }

        [Fact]
        public void SetFromAttributes_BrowsableAttribute_ShouldSetBrowsable()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed");

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Browsable.Should().BeFalse();
        }

        [Fact]
        public void Set_WhenWritableProperty_ShouldCallSetValue()
        {
            var obj = new PlainPropertyHolder { Plain = "original" };
            var prop = typeof(PlainPropertyHolder).GetProperty("Plain");
            var entry = new PropertySettingEntry(obj, prop, null);

            // Writable property: ReadOnly = !CanWrite = false, so Set succeeds
            entry.Set("newValue");

            obj.Plain.Should().Be("newValue");
        }

        [Fact]
        public void Set_WhenReadOnlyProperty_ShouldNotCallSetValue()
        {
            var obj = new NonWritableHolder("original");
            var prop = typeof(NonWritableHolder).GetProperty("ReadOnlyValue");
            var entry = new PropertySettingEntry(obj, prop, null);

            // Non-writable property: ReadOnly = !CanWrite = true, so Set is blocked
            entry.Set("newValue");

            obj.ReadOnlyValue.Should().Be("original");
        }

        [Fact]
        public void SetFromAttributes_UnrecognizedAttributeBeforeKnownOne_ShouldNotSkipRemaining()
        {
            var obj = new PlainPropertyHolder();
            var prop = typeof(PlainPropertyHolder).GetProperty("Plain");

            object[] attribs = [new object(), new DescriptionAttribute("Should still be applied")];

            var entry = new PropertySettingEntry(obj, prop, null);
            entry.SetFromAttributes(attribs, null);

            entry.Description.Should().Be("Should still be applied");
        }
    }
}
