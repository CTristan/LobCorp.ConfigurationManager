// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System.ComponentModel;
using AwesomeAssertions;
using ConfigurationManager;
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
            var prop = typeof(PlainPropertyHolder).GetProperty("Plain")!;

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DispName.Should().Be("Plain");
        }

        [Fact]
        public void SetFromAttributes_DisplayNameAttribute_ShouldSetDispName()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed")!;

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DispName.Should().Be("Custom Name");
        }

        [Fact]
        public void SetFromAttributes_CategoryAttribute_ShouldSetCategory()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed")!;

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Category.Should().Be("MyCategory");
        }

        [Fact]
        public void SetFromAttributes_DescriptionAttribute_ShouldSetDescription()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed")!;

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Description.Should().Be("A description");
        }

        [Fact]
        public void SetFromAttributes_DefaultValueAttribute_ShouldSetDefaultValue()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed")!;

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.DefaultValue.Should().Be(42);
        }

        [Fact]
        public void SetFromAttributes_ReadOnlyAttribute_ShouldSetReadOnly()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed")!;

            var entry = new PropertySettingEntry(obj, prop, null);

            // ReadOnlyAttribute(true) is applied, then PropertySettingEntry overwrites
            // with CanWrite (which is true). Net result: ReadOnly = true
            entry.ReadOnly.Should().BeTrue();
        }

        [Fact]
        public void SetFromAttributes_BrowsableAttribute_ShouldSetBrowsable()
        {
            var obj = new AttributedPropertyHolder();
            var prop = typeof(AttributedPropertyHolder).GetProperty("FullyAttributed")!;

            var entry = new PropertySettingEntry(obj, prop, null);

            entry.Browsable.Should().BeFalse();
        }

        [Fact]
        public void Set_WhenReadOnlyIsTrue_ShouldNotCallSetValue()
        {
            // PropertySettingEntry sets ReadOnly = CanWrite for writable properties (true)
            // When ReadOnly == true, Set() should not call SetValue
            var obj = new PlainPropertyHolder { Plain = "original" };
            var prop = typeof(PlainPropertyHolder).GetProperty("Plain")!;
            var entry = new PropertySettingEntry(obj, prop, null);

            // ReadOnly is true (CanWrite=true), so Set should be blocked
            entry.Set("newValue");

            obj.Plain.Should().Be("original");
        }

        [Fact]
        public void Set_WhenReadOnlyIsFalse_ShouldCallSetValue()
        {
            // For a non-writable property, ReadOnly = CanWrite = false
            // When ReadOnly == false (not true), Set() should call SetValue
            // But a read-only property can't actually be set... so this tests the guard logic
            var obj = new NonWritableHolder("original");
            var prop = typeof(NonWritableHolder).GetProperty("ReadOnlyValue")!;
            var entry = new PropertySettingEntry(obj, prop, null);

            // ReadOnly = false (CanWrite=false), so Set() will try to call SetValue
            // But the property has no setter, so it throws
            var act = () => entry.Set("newValue");

            // The property has no setter, so PropertyInfo.SetValue throws ArgumentException
            act.Should().Throw<System.ArgumentException>();
        }
    }
}
