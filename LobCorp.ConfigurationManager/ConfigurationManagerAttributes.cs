// SPDX-License-Identifier: LGPL-3.0-or-later

#pragma warning disable 0169, 0414, 0649
namespace ConfigurationManager
{
    /// <summary>
    /// <para>Class that specifies how a setting should be displayed inside the ConfigurationManager settings window.</para>
    /// <para>
    /// Usage:
    /// This class template has to be copied inside the plugin's project and referenced by its code directly.
    /// Make a new instance, assign any fields that you want to override, and pass it as a tag for your setting.
    /// </para>
    /// <para>
    /// If a field is null (default), it will be ignored and won't change how the setting is displayed.
    /// If a field is non-null (you assigned a value to it), it will override default behavior.
    /// </para>
    /// </summary>
    ///
    /// <example>
    /// Here's an example of overriding order of settings and marking one of the settings as advanced:
    /// <code>
    /// var configFile = LmmConfigRegistration.GetConfigFile("MyMod", "My Mod");
    /// configFile.Bind("X", "1", 1, new LmmConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
    /// configFile.Bind("X", "2", 2, new LmmConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
    /// configFile.Bind("X", "3", 3, new LmmConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
    /// </code>
    /// </example>
    ///
    /// <remarks>
    /// You can read more and see examples in the readme at https://github.com/BepInEx/BepInEx.ConfigurationManager
    /// You can optionally remove fields that you won't use from this class, it's the same as leaving them null.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
        Justification = "Internal data holder class copied by plugins"
    )]
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>
        /// Should the setting be shown as a percentage (only use with value range settings).
        /// </summary>
        public bool? ShowRangeAsPercent;

        /// <summary>
        /// Custom setting editor (OnGUI code that replaces the default editor provided by ConfigurationManager).
        /// See below for a deeper explanation. Using a custom drawer will cause many of the other fields to do nothing.
        /// </summary>
        public System.Action<Config.LmmConfigEntryBase> CustomDrawer;

        /// <summary>
        /// Custom setting editor that allows polling keyboard input with the Input class.
        /// Use either CustomDrawer or CustomHotkeyDrawer, using both at the same time leads to undefined behaviour.
        /// </summary>
        public CustomHotkeyDrawerFunc CustomHotkeyDrawer;

        /// <summary>
        /// Custom setting draw action that allows polling keyboard input with the Input class.
        /// </summary>
        /// <param name="setting">The config entry being drawn in the settings window.</param>
        /// <param name="isCurrentlyAcceptingInput">Ref flag indicating whether the control is actively capturing keyboard input.</param>
        public delegate void CustomHotkeyDrawerFunc(
            Config.LmmConfigEntryBase setting,
            ref bool isCurrentlyAcceptingInput
        );

        /// <summary>
        /// Show this setting in the settings screen at all? If false, don't show.
        /// </summary>
        public bool? Browsable;

        /// <summary>
        /// Category the setting is under. Null to be directly under the plugin.
        /// </summary>
        public string Category;

        /// <summary>
        /// If set, a "Default" button will be shown next to the setting to allow resetting to default.
        /// </summary>
        public object DefaultValue;

        /// <summary>
        /// Force the "Reset" button to not be displayed, even if a valid DefaultValue is available.
        /// </summary>
        public bool? HideDefaultButton;

        /// <summary>
        /// Force the setting name to not be displayed.
        /// </summary>
        public bool? HideSettingName;

        /// <summary>
        /// Optional description shown when hovering over the setting.
        /// </summary>
        public string Description;

        /// <summary>
        /// Name of the setting.
        /// </summary>
        public string DispName;

        /// <summary>
        /// Order of the setting on the settings list relative to other settings in a category.
        /// 0 by default, higher number is higher on the list.
        /// </summary>
        public int? Order;

        /// <summary>
        /// Only show the value, don't allow editing it.
        /// </summary>
        public bool? ReadOnly;

        /// <summary>
        /// If true, don't show the setting by default. User has to turn on showing advanced settings or search for it.
        /// </summary>
        public bool? IsAdvanced;

        /// <summary>
        /// Custom converter from setting type to string for the built-in editor textboxes.
        /// </summary>
        public System.Func<object, string> ObjToStr;

        /// <summary>
        /// Custom converter from string to setting type for the built-in editor textboxes.
        /// </summary>
        public System.Func<string, object> StrToObj;
    }
}
